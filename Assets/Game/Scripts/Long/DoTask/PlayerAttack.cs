using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    // origin transform (fire point). Nếu null sẽ dùng transform của object này.
    public Transform playerPrefab;

    // prefab to spawn each attack (animation / VFX / hitbox object). Can be a ParticleSystem, animated prefab, or projectile.
    public GameObject attackEffectPrefab;

    // optional camera to determine attack direction. If null will use Camera.main.
    public Camera attackCamera;

    [Header("Timing")]
    // time between automatic attacks (seconds)
    public float attackDelay = 0.5f;

    // whether auto attack runs automatically. You can toggle at runtime.
    public bool autoAttack = true;

    // internal timer
    float attackTimer = 0f;

    [Header("Spawn positioning")]
    // how far outside the capsule/renderer to spawn
    public float spawnOffset = 0.12f;

    // small vertical offset added to computed chest height
    public float spawnHeightOffset = 0.0f;

    // layer mask for raycasts when checking front obstacles (optional)
    public LayerMask obstacleMask = ~0; // default everything

    void Start()
    {
        if (playerPrefab == null) playerPrefab = transform;
        if (attackCamera == null) attackCamera = Camera.main;
        attackTimer = attackDelay; // allow immediate attack on start; change to 0 if want to wait first delay
    }

    void Update()
    {
        if (!autoAttack || attackEffectPrefab == null || attackCamera == null) 
        {
            // still tick timer so enabling later will not attack instantly unexpectedly
            attackTimer = Mathf.Min(attackTimer + Time.deltaTime, attackDelay);
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDelay)
        {
            attackTimer = 0f;
            MeleeAttack();
        }
    }

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
        Vector3 rayOrigin = fp.position + Vector3.up * 0.2f;
        if (Physics.Raycast(rayOrigin, dirFlat, out hit, moveDist + 0.1f, obstacleMask))
        {
            // there's something right in front, push spawn to hit.point + small gap
            spawn = hit.point + dirFlat * 0.12f;
            spawn.y = chestY;
        }

        return spawn;
    }

    void MeleeAttack()
    {
        // compute direction from camera forward (full direction including pitch)
        Vector3 camDir = attackCamera.transform.forward;

        // horizontal only
        Vector3 dirFlat = new Vector3(camDir.x, 0f, camDir.z);
        if (dirFlat.sqrMagnitude < 0.0001f) 
            dirFlat = playerPrefab.forward;
        dirFlat.Normalize();

        // compute spawn pos using your existing logic
        Vector3 spawnPos = ComputeSpawnPosition(playerPrefab, dirFlat);

        // rotation also horizontal only
        Quaternion rot = Quaternion.LookRotation(dirFlat, Vector3.up);

        Quaternion offset = Quaternion.Euler(90f, 0f, 0f);
        // instantiate effect prefab
        GameObject go = Instantiate(attackEffectPrefab, spawnPos, rot* offset);
        Destroy(go, 0.4f);

        // optional: if prefab expects to receive initial velocity / direction, try to pass it
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // small forward impulse so e.g. a projectile moves; change multiplier as needed
            rb.linearVelocity = camDir * 8f;
        }

        // if the prefab contains a script named "AttackEffect" that expects initialization,
        // try to call an Init method (optional pattern).
        var init = go.GetComponent<IAttackEffect>();
        if (init != null)
        {
            init.Init(camDir, playerPrefab);
        }
    }
}

// Optional interface the prefab might implement to receive init parameters.
// This is just a convenience -- create this in a separate file if you want to use it.
public interface IAttackEffect
{
    void Init(Vector3 dir, Transform owner);
}
