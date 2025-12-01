using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponUpgradeData weaponData;  // gán data vũ khí vào đây

    [Header("References")]
    public Transform playerPrefab;
    public GameObject attackEffectPrefab;
    public Camera attackCamera;

    float attackTimer = 0f;

    [Header("Spawn positioning")]
    public float spawnOffset = 0.12f;
    public float spawnHeightOffset = 0.0f;
    public LayerMask obstacleMask;

    public bool autoAttack = true;

    [Header("Debug")]
    public bool DebugTest = true;

    void Start()
    {
        if (playerPrefab == null) playerPrefab = transform;
        if (attackCamera == null) attackCamera = Camera.main;

        attackTimer = weaponData.attackDelay;
    }

    void Update()
    {
        if (!autoAttack || attackEffectPrefab == null || attackCamera == null)
        {
            attackTimer = Mathf.Min(attackTimer + Time.deltaTime, weaponData.attackDelay);
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= weaponData.attackDelay)
        {
            attackTimer = 0f;
            MeleeAttack();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            WeaponUpgrade.Upgrade(weaponData,RarityHelper.GetRandomRarity());
            Debug.Log($"Damage: {weaponData.damage} \n Range: {weaponData.range} \n Speed: {weaponData.attackSpeed} ");
        }

    }

    Vector3 ComputeSpawnPosition(Transform fp, Vector3 dir)
    {
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

    private void MeleeAttack()
    {
        Vector3 camDir = attackCamera.transform.forward;

        Vector3 dirFlat = new Vector3(camDir.x, 0f, camDir.z);

        if (dirFlat.sqrMagnitude < 0.0001f)
            dirFlat = playerPrefab.forward;

        dirFlat.Normalize();

        
        Vector3 spawnPos = ComputeSpawnPosition(playerPrefab, dirFlat);

        Quaternion rot = Quaternion.LookRotation(dirFlat, Vector3.up);
        Quaternion offset = Quaternion.Euler(90f, 0f, -60f);

        GameObject go = Instantiate(attackEffectPrefab, spawnPos, rot * offset);

        // Vector3 center = transform.position + spawnPos * (weaponData.range * 0.5f);
        Collider[] hits = Physics.OverlapSphere(spawnPos, weaponData.range, obstacleMask);

        if(DebugTest)
        {
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * Mathf.PI * 2f / segments;
                float angle2 = (i + 1) * Mathf.PI * 2f / segments;
                
                Vector3 point1 = spawnPos + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * weaponData.range;
                Vector3 point2 = spawnPos + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * weaponData.range;
                Debug.DrawLine(point1, point2, Color.cyan, 0.1f); // 0.1s để vẽ tạm thời

            }
        }
        foreach (var c in hits)
        {
            if (c == null) continue;
            // Debug.DrawLine(spawnPos, c.bounds.center, Color.red, 0.1f);
            if (c.gameObject == gameObject) continue;
            var enemy = c.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }

        Destroy(go, 0.4f);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = camDir * 8f;
        }

    }
}
