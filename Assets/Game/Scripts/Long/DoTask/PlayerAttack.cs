using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;    // transform của player
    public Camera attackCamera;          // camera để lấy hướng tấn công

    [Header("Spawn positioning")]
    public float spawnOffset = 0.12f;
    public float spawnHeightOffset = 0.0f;
    public LayerMask obstacleMask;
    
    public Weapon wp;

    void Start()
    {
        if (playerTransform == null)
            playerTransform = transform;

        if (attackCamera == null)
            attackCamera = Camera.main;
        WeaponManager.Instance.AddWeapon(wp);

    }
    public Vector3 ComputeSpawnPosition(Vector3 dir)
    {
        Transform fp = playerTransform;

        Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);

        if (dirFlat.sqrMagnitude < 0.0001f)
            dirFlat = fp.forward;

        dirFlat.Normalize();

        CapsuleCollider cap = fp.GetComponent<CapsuleCollider>();

        float radiusWorld = 0.5f;
        float heightWorld = 1.0f;
        Vector3 centerWorld = fp.position;

        if (cap != null)
        {
            Vector3 lossy = fp.lossyScale;
            float scaleXZ = Mathf.Max(lossy.x, lossy.z);
            radiusWorld = cap.radius * scaleXZ;
            heightWorld = cap.height * lossy.y;
            centerWorld = fp.TransformPoint(cap.center);
        }
        else
        {
            var rend = fp.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                radiusWorld = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.z);
                heightWorld = rend.bounds.size.y;
                centerWorld = rend.bounds.center;
            }
        }

        float moveDist = radiusWorld + spawnOffset;
        Vector3 spawn = centerWorld + dirFlat * moveDist;

        float chestY = fp.position.y + Mathf.Clamp(heightWorld * 0.25f, 0.2f, 1.2f) + spawnHeightOffset;
        spawn.y = chestY;

        RaycastHit hit;
        Vector3 rayOrigin = fp.position + Vector3.up * 0.2f;

        if (Physics.Raycast(rayOrigin, dirFlat, out hit, moveDist + 0.1f, obstacleMask))
        {
            spawn = hit.point + dirFlat * 0.12f;
            spawn.y = chestY;
        }

        return spawn;
    }

    public Vector3 GetForwardDirection()
    {
        // hướng tấn công do camera quyết định
        Vector3 camDir = attackCamera.transform.forward;
        return new Vector3(camDir.x, 0f, camDir.z).normalized;
    }
}

// using UnityEditor.Build;
// using UnityEngine;

// public class PlayerAttack : MonoBehaviour
// {
//     [Header("Weapon Data")]
//     public WeaponUpgradeData weaponData;  // gán data vũ khí vào đây

//     [Header("References")]
//     public Transform playerPrefab;
//     public GameObject attackEffectPrefab;
//     public Camera attackCamera;

//     float attackTimer = 0f;

//     [Header("Spawn positioning")]
//     public float spawnOffset = 0.12f;
//     public float spawnHeightOffset = 0.0f;
//     public LayerMask obstacleMask;

//     public bool autoAttack = true;

//     [Header("Debug")]
//     public bool DebugTest = true;
//     private Vector3 debugBoxCenter;
//     private Vector3 debugBoxSize;
//     private Quaternion debugBoxRot;
//     private bool debugDrawBox = false;

//     void Start()
//     {
//         if (playerPrefab == null) playerPrefab = transform;
//         if (attackCamera == null) attackCamera = Camera.main;

//         attackTimer = weaponData.attackDelay;
//     }

//     void Update()
//     {
//         if (!autoAttack || attackEffectPrefab == null || attackCamera == null)
//         {
//             attackTimer = Mathf.Min(attackTimer + Time.deltaTime, weaponData.attackDelay);
//             return;
//         }

//         attackTimer += Time.deltaTime;

//         if (attackTimer >= weaponData.attackDelay)
//         {
//             attackTimer = 0f;
//             MeleeAttack();
//         }
//         if(Input.GetKeyDown(KeyCode.U))
//         {
//             WeaponUpgrade.Upgrade(weaponData,RarityHelper.GetRandomRarity());
//             Debug.Log($"Damage: {weaponData.damage} \n Range: {weaponData.range} \n Speed: {weaponData.attackSpeed} ");
//         }

//     }

//     Vector3 ComputeSpawnPosition(Transform fp, Vector3 dir)
//     {
//         Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);

//         if (dirFlat.sqrMagnitude < 0.0001f)
//             dirFlat = fp.forward;

//         dirFlat.Normalize();

//         CapsuleCollider cap = fp.GetComponent<CapsuleCollider>();

//         float radiusWorld = 0.5f;
//         float heightWorld = 1.0f;
//         Vector3 centerWorld = fp.position;

//         if (cap != null)
//         {
//             Vector3 lossy = fp.lossyScale;
//             float scaleXZ = Mathf.Max(lossy.x, lossy.z);
//             radiusWorld = cap.radius * scaleXZ;
//             heightWorld = cap.height * lossy.y;
//             centerWorld = fp.TransformPoint(cap.center);
//         }
//         else
//         {
//             var rend = fp.GetComponentInChildren<Renderer>();
//             if (rend != null)
//             {
//                 radiusWorld = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.z);
//                 heightWorld = rend.bounds.size.y;
//                 centerWorld = rend.bounds.center;
//             }
//         }

//         float moveDist = radiusWorld + spawnOffset;
//         Vector3 spawn = centerWorld + dirFlat * moveDist;

//         float chestY = fp.position.y + Mathf.Clamp(heightWorld * 0.25f, 0.2f, 1.2f) + spawnHeightOffset;
//         spawn.y = chestY;

//         RaycastHit hit;
//         Vector3 rayOrigin = fp.position + Vector3.up * 0.2f;

//         if (Physics.Raycast(rayOrigin, dirFlat, out hit, moveDist + 0.1f, obstacleMask))
//         {
//             spawn = hit.point + dirFlat * 0.12f;
//             spawn.y = chestY;
//         }

//         return spawn;
//     }

//     private void MeleeAttack()
//     {
//         Vector3 camDir = attackCamera.transform.forward;

//         Vector3 dirFlat = new Vector3(camDir.x, 0f, camDir.z);

//         if (dirFlat.sqrMagnitude < 0.0001f)
//             dirFlat = playerPrefab.forward;

//         dirFlat.Normalize();

        
//         Vector3 spawnPos = ComputeSpawnPosition(playerPrefab, dirFlat);

//         Quaternion rot = Quaternion.LookRotation(dirFlat, Vector3.up);
//         Quaternion offset = Quaternion.Euler(90f, 0f, 0f);

//         GameObject go = Instantiate(attackEffectPrefab, spawnPos, rot * offset);

//         ParticleSystem ps = go.GetComponent<ParticleSystem>();

//         if (ps != null)
//         {           
//             // Tự động destroy sau khi particle chạy xong
//             float particleDuration = ps.main.duration + ps.main.startLifetime.constantMax;
//             Destroy(go, particleDuration);
//         }
//         else
//         {
//             // Fallback nếu không tìm thấy ParticleSystem
//             Destroy(go, 0.4f);
//         }

//         // Vector3 center = transform.position + spawnPos * (weaponData.range * 0.5f);
//         // Collider[] hits = Physics.OverlapSphere(spawnPos, weaponData.range, obstacleMask);

//         // --- HITBOX ---
//         Vector3 size = new Vector3(weaponData.range, 0.05f, weaponData.range);
//         Collider[] hits = Physics.OverlapBox(spawnPos, size * 0.5f, rot, obstacleMask);
//         if(DebugTest)
//         {
//             debugBoxCenter = spawnPos;
//             debugBoxSize = size;
//             debugBoxRot = rot;
//             debugDrawBox = true;
//         }
//         else
//         {
//             debugDrawBox = false;
//         }
//         foreach (var c in hits)
//         {
//             if (c == null) continue;
//             // Debug.DrawLine(spawnPos, c.bounds.center, Color.red, 0.1f);
//             if (c.gameObject == gameObject) continue;
//             var enemy = c.GetComponent<EnemyAI>();
//             if (enemy != null)
//             {
//                 enemy.TakeDamage(weaponData.damage);
//             }
//         }

//         // Destroy(go, 0.4f);

//         var rb = go.GetComponent<Rigidbody>();
//         if (rb != null)
//         {
//             rb.linearVelocity = camDir * 8f;
//         }

//     }
//     private void OnDrawGizmos()
//     {
//         if (!debugDrawBox) return;

//         Gizmos.color = Color.red;
//         Gizmos.matrix = Matrix4x4.TRS(debugBoxCenter, debugBoxRot, Vector3.one);
//         Gizmos.DrawWireCube(Vector3.zero, debugBoxSize);
//     }
// }
