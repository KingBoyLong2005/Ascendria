using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class PlayerShooting : MonoBehaviour
{
    [Header("General")]
    public Transform firePoint; // vị trí spawn projectile (nếu null sẽ dùng transform)
    public LayerMask enemyLayer; // set layer Enemy
    public bool useAllActiveWeapons = true; // true: dùng tất cả active weapons khi click; false: dùng weapon theo index
    [Tooltip("If not using all weapons, this index will be used (0-based).")]
    public int singleWeaponIndex = 0;

    [Header("Input")]
    public bool holdToAutoFire = false; // true để giữ chuột auto kích hoạt
    public float autoFireInterval = 0.02f; // interval check khi hold

    [Header("Projectile Settings")]
    public float defaultProjectileSpeed = 20f; // nếu prefab không có script Rigidbody

    // small offsets used when spawning outside the player's capsule
    [Tooltip("How far outside the player's capsule the projectile should spawn (in world units).")]
    public float spawnOffset = 0.12f;
    [Tooltip("Vertical offset from player position (useful to spawn at chest height).")]
    public float spawnHeightOffset = 0.2f;

    // cooldown tracker per weapon data (keyed by WeaponData reference)
    private Dictionary<WeaponData, float> lastUseTime = new Dictionary<WeaponData, float>();

    Camera mainCam;
    float lastAutoFireTime = 0f;

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        bool pressed = Input.GetMouseButtonDown(0);
        bool held = Input.GetMouseButton(0);

        if (pressed || (holdToAutoFire && held && Time.time >= lastAutoFireTime + autoFireInterval))
        {
            Vector3 aim = GetAimDirection();
            UseActiveWeapons(aim);
            if (holdToAutoFire && held) lastAutoFireTime = Time.time;
        }
    }

    Vector3 GetAimDirection()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return transform.forward;
        }

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            return (hit - transform.position).normalized;
        }
        return transform.forward;
    }

    void UseActiveWeapons(Vector3 aimDirection)
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.activeWeapons == null || inv.activeWeapons.Count == 0) return;

        var processed = new HashSet<WeaponData>();
        if (useAllActiveWeapons)
        {
            for (int i = 0; i < inv.activeWeapons.Count; i++)
            {
                var w = inv.activeWeapons[i];
                if (w == null) continue;
                if (processed.Contains(w)) continue; // skip duplicate ref
                processed.Add(w);
                TryUseWeapon(w, aimDirection);
            }
        }
        else
        {
            if (singleWeaponIndex >= 0 && singleWeaponIndex < inv.activeWeapons.Count)
            {
                var w = inv.activeWeapons[singleWeaponIndex];
                if (w != null && !processed.Contains(w))
                {
                    processed.Add(w);
                    TryUseWeapon(w, aimDirection);
                }
            }
        }
    }

    void TryUseWeapon(WeaponData wdata, Vector3 aimDirection)
    {
        if (wdata == null) return;

        float now = Time.time;
        if (!lastUseTime.TryGetValue(wdata, out float last)) last = -9999f;

        // attackRate = lần/giây -> cooldown = 1/attackRate
        float cooldown = 1f / Mathf.Max(0.0001f, wdata.attackRate);
        if (now < last + cooldown) return;

        // IMPORTANT: check your enum name. Replace WeaponTypeLong by the actual enum name (WeaponType).
        switch (wdata.weaponType)
        {
            case WeaponTypeLong.Projectile:
                UseProjectile(wdata, aimDirection);
                break;
            case WeaponTypeLong.Melee:
                UseMelee(wdata, aimDirection);
                break;
            case WeaponTypeLong.Throw:
                UseThrow(wdata, aimDirection);
                break;
            case WeaponTypeLong.AoE:
                UseAoE(wdata);
                break;
            default:
                Debug.LogWarning("Unknown weapon type: " + wdata.weaponType);
                break;
        }

        lastUseTime[wdata] = now;
    }

    #region Weapon Implementations

    // Helper: compute a spawn position outside the capsule collider of the firePoint (which may be the player)
    Vector3 ComputeSpawnPosition(Transform fp, Vector3 dir)
    {
        // ensure horizontal direction
        Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);
        if (dirFlat.sqrMagnitude < 0.0001f) dirFlat = fp.forward;
        dirFlat.Normalize();

        // try to get a CapsuleCollider from the firePoint (commonly the player)
        CapsuleCollider cap = fp.GetComponent<CapsuleCollider>();
        float radiusWorld = 0.5f; // fallback
        float heightWorld = 1.0f;
        Vector3 centerWorld = fp.position;

        if (cap != null)
        {
            // account for local scale on X/Z for radius
            Vector3 lossy = fp.lossyScale;
            float scaleXZ = Mathf.Max(lossy.x, lossy.z);
            radiusWorld = cap.radius * scaleXZ;
            heightWorld = cap.height * lossy.y;
            centerWorld = fp.TransformPoint(cap.center);
        }
        else
        {
            // fallback: try to use any renderer bounds
            var rend = fp.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                radiusWorld = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.z);
                heightWorld = rend.bounds.size.y;
                centerWorld = rend.bounds.center;
            }
        }

        // compute spawn position: move from center by radius + small offset along dir
        float moveDist = radiusWorld + spawnOffset;
        Vector3 spawn = centerWorld + dirFlat * moveDist;

        // set spawn.y so projectile spawns roughly at chest height (player Y + half height * some factor)
        float chestY = fp.position.y + Mathf.Clamp(heightWorld * 0.25f, 0.2f, 1.2f) + spawnHeightOffset;
        spawn.y = chestY;

        // raycast forward a bit to avoid spawning inside walls; if blocked, push spawn a little further out
        RaycastHit hit;
        if (Physics.Raycast(fp.position + Vector3.up * 0.2f, dirFlat, out hit, moveDist + 0.1f))
        {
            // there's something right in front, push spawn to hit.point + small gap
            spawn = hit.point + dirFlat * 0.12f;
            spawn.y = chestY;
        }

        return spawn;
    }

    void UseProjectile(WeaponData wdata, Vector3 aimDirection)
    {
        if (wdata.prefab == null)
        {
            Debug.LogWarning($"Projectile weapon '{wdata.weaponName}' has no prefab assigned.");
            return;
        }

        // compute spawn position outside player's capsule so it won't push the player
        Vector3 dir = aimDirection;
        Vector3 spawnPos = ComputeSpawnPosition(firePoint, dir);
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z).sqrMagnitude < 0.0001f ? firePoint.forward : new Vector3(dir.x, 0f, dir.z));

        GameObject instance = Instantiate(wdata.prefab, spawnPos, rot);

        // nếu prefab có component Projectile (custom), gọi Initialize
        var proj = instance.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(aimDirection, wdata.baseDamage, gameObject);
        }

        // nếu có rigidbody, set velocity (sử dụng rb.velocity)
        var rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = aimDirection.normalized * defaultProjectileSpeed;
        }
        else
        {
            // nếu prefab không có rigidbody, destroy sau một thời gian để tránh rác
            Destroy(instance, 5f);
        }

        // avoid immediate collision with the player if both have colliders
        // try player capsule collider first (assumed on the same GameObject as this script)
        CapsuleCollider playerCap = GetComponent<CapsuleCollider>();
        Collider projCol = instance.GetComponent<Collider>();
        if (projCol != null && playerCap != null)
        {
            Physics.IgnoreCollision(projCol, playerCap, true);
            // re-enable later if desired (not implemented here). Using layers is recommended for larger projects.
        }
    }

    void UseMelee(WeaponData wdata, Vector3 aimDirection)
    {
        Vector3 center = transform.position + aimDirection.normalized * (wdata.range * 0.5f);
        float radius = wdata.range;
        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);

        // determine direction to nearest enemy if exist (for orienting VFX)
        Vector3 aimTo = aimDirection;
        if (hits.Length > 0)
        {
            Collider nearest = null;
            float best = float.MaxValue;
            foreach (var c in hits)
            {
                if (c == null) continue;
                float d = (c.transform.position - center).sqrMagnitude;
                if (d < best) { best = d; nearest = c; }
            }
            if (nearest != null) aimTo = (nearest.transform.position - center).normalized;
        }

        // spawn VFX or hitbox prefab (if assigned). Make sure prefab's collider/Rigidbody are appropriate:
        if (wdata.prefab != null)
        {
            Vector3 spawnPos = ComputeSpawnPosition(firePoint, aimTo);
            Quaternion rot = Quaternion.LookRotation(new Vector3(aimTo.x, 0f, aimTo.z).sqrMagnitude < 0.0001f ? firePoint.forward : new Vector3(aimTo.x, 0f, aimTo.z));
            // Nếu prefab "facing up" thay vì facing forward, chỉnh offset. Thử thay offset nếu không đúng.
            Quaternion offset = Quaternion.Euler(90f, 0f, 0f);
            GameObject vfx = Instantiate(wdata.prefab, spawnPos, rot * offset);
            Destroy(vfx, 0.9f);
        }

        // apply damage
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (c.gameObject == gameObject) continue;
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(wdata.baseDamage);
            }
        }
    }

    void UseThrow(WeaponData wdata, Vector3 aimDirection)
    {
        if (wdata.prefab == null)
        {
            Debug.LogWarning($"Throw weapon '{wdata.weaponName}' has no prefab assigned.");
            return;
        }

        Vector3 dir = aimDirection;
        Vector3 spawnPos = ComputeSpawnPosition(firePoint, dir);
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z).sqrMagnitude < 0.0001f ? firePoint.forward : new Vector3(dir.x, 0f, dir.z));

        GameObject instance = Instantiate(wdata.prefab, spawnPos, rot);
        var rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 vdir = (aimDirection + Vector3.up * 0.2f).normalized;
            rb.linearVelocity = vdir * defaultProjectileSpeed;
        }

        var proj = instance.GetComponent<Projectile>();
        if (proj != null)
            proj.Initialize(aimDirection, wdata.baseDamage, gameObject);

        // avoid immediate collision with the player
        CapsuleCollider playerCap = GetComponent<CapsuleCollider>();
        Collider projCol = instance.GetComponent<Collider>();
        if (projCol != null && playerCap != null)
        {
            Physics.IgnoreCollision(projCol, playerCap, true);
        }
    }

    void UseAoE(WeaponData wdata)
    {
        Vector3 center = transform.position;
        float radius = wdata.range;
        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);

        if (wdata.prefab != null)
        {
            GameObject vfx = Instantiate(wdata.prefab, center, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }

        foreach (var c in hits)
        {
            if (c == null) continue;
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(wdata.baseDamage);
            }
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.activeWeapons == null) return;

        if (useAllActiveWeapons)
        {
            foreach (var w in InventoryManager.Instance.activeWeapons)
            {
                if (w == null) continue;
                if (w.weaponType == WeaponTypeLong.Melee || w.weaponType == WeaponTypeLong.AoE)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, w.range);
                }
            }
        }
        else
        {
            if (singleWeaponIndex >= 0 && singleWeaponIndex < InventoryManager.Instance.activeWeapons.Count)
            {
                var w = InventoryManager.Instance.activeWeapons[singleWeaponIndex];
                if (w != null && (w.weaponType == WeaponTypeLong.Melee || w.weaponType == WeaponTypeLong.AoE))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, w.range);
                }
            }
        }
    }
}
