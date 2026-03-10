using UnityEngine;
using TMPro;

public class DiceProjectile : MonoBehaviour
{
    private float baseDamage;
    private float speed;
    private Vector3 direction;
    private int rolledNumber;
    private LayerMask enemyMask;
    private float aoeSize;
    private float luckValue;

    private TextMeshPro numberText;
    private GameObject textObj;
    private Transform playerTransform; // Lưu reference đến player
    
    private float lifetime = 5f;
    private float lifetimeTimer = 0f;
    private bool hasHit = false;

    public void Initialize(float baseDmg, float projectileSpeed, Vector3 dir, LayerMask mask, float aoeRadius, float luck)
    {
        baseDamage = baseDmg;
        speed = projectileSpeed;
        direction = dir.normalized;
        enemyMask = mask;
        aoeSize = aoeRadius;
        luckValue = luck;
        hasHit = false;
        lifetimeTimer = 0f;

        // Lấy player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Roll dice (1-20) with luck influence
        rolledNumber = RollDiceWithLuck(luck);
        
        // Display number on dice
        CreateNumberDisplay();
    }

    private int RollDiceWithLuck(float luck)
    {
        // Base roll: 1-20
        // Luck increases chance of higher numbers
        // Formula: weighted random based on luck
        
        float roll = Random.value; // 0.0 to 1.0
        
        // Apply luck bias: higher luck shifts the roll toward higher values
        // Luck effect: 0 luck = no bias, higher luck = more bias toward max
        float luckBias = Mathf.Clamp01(luck / 100f); // Normalize luck (assuming luck 0-100 range)
        float biasedRoll = Mathf.Lerp(roll, roll * roll, luckBias); // Square function for higher numbers
        
        // Convert to 1-20 range
        int result = Mathf.FloorToInt(biasedRoll * 20f) + 1;
        result = Mathf.Clamp(result, 1, 20);
        
        return result;
    }

    private void CreateNumberDisplay()
    {
        // Create TextMeshPro to display number
        textObj = new GameObject("DiceNumber");
        
        // Set parent vào player (child của player)
        if (playerTransform != null)
        {
            textObj.transform.SetParent(playerTransform);
            
            // Lấy collider để tính chiều cao giữa
            Collider playerCollider = playerTransform.GetComponent<Collider>();
            float playerHeight = playerCollider != null ? playerCollider.bounds.size.y : 2f;
            
            // Local position: bên phải (0.5 units) + giữa chiều cao
            // Dùng local vì đã là child của player
            textObj.transform.localPosition = Vector3.right * 0.5f + Vector3.up * (playerHeight * 0.5f);
        }
        else
        {
            // Fallback: nếu không tìm thấy player
            textObj.transform.position = transform.position + Vector3.up * 0.5f;
        }
        
        numberText = textObj.AddComponent<TextMeshPro>();
        numberText.text = rolledNumber.ToString();
        numberText.fontSize = 4;
        numberText.alignment = TextAlignmentOptions.Center;
        
        // Color based on roll result
        if (rolledNumber == 1)
            numberText.color = Color.red; // Critical fail
        else if (rolledNumber == 20)
            numberText.color = Color.yellow; // Critical success (AOE)
        else
            numberText.color = Color.white;
        
        // Billboard effect - always face camera
        if (Camera.main != null)
            textObj.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        
        // Scale text
        textObj.transform.localScale = Vector3.one * 0.5f;
    }

    private void Update()
    {
        // Move projectile
        transform.position += direction * speed * Time.deltaTime;
        
        // Chỉ cần update billboard rotation, không cần update position nữa vì text đã là child của player
        if (numberText != null && Camera.main != null)
        {
            textObj.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
        
        // Rotate dice for visual effect
        transform.Rotate(Vector3.right * 360f * Time.deltaTime);
        
        // Lifetime check - use PoolManager instead of Destroy
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetime && !hasHit)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Check if hit enemy
        if (((1 << other.gameObject.layer) & enemyMask) != 0)
        {
            var enemy = other.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                hasHit = true;
                
                // Calculate damage based on rolled number
                if (rolledNumber == 1)
                {
                    // Critical fail: only 1 damage
                    DealSingleDamage(enemy, 1f);
                }
                else if (rolledNumber == 20)
                {
                    // Critical success: AOE damage
                    DealAOEDamage(enemy.transform.position);
                }
                else
                {
                    // Normal: Base Damage + rolled number
                    float finalDamage = baseDamage + rolledNumber;
                    DealSingleDamage(enemy, finalDamage);
                }
                
                ReturnToPool();
            }
        }
    }

    private void DealSingleDamage(EnemyStats enemy, float damage)
    {
        WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
        
        string colorCode = rolledNumber == 1 ? "red" : "cyan";
        Debug.Log($"<color={colorCode}>🎲 Dice rolled {rolledNumber} and dealt {damage} damage to {enemy.name}</color>");
    }

    private void DealAOEDamage(Vector3 center)
    {
        // Get all enemies in AOE radius
        Collider[] hitEnemies = Physics.OverlapSphere(center, aoeSize, enemyMask);
        
        // Calculate AOE damage: (Base Damage + 20) * 2
        float aoeDamage = (baseDamage + 20f) * 2f;
        
        int enemiesHit = 0;
        foreach (var collider in hitEnemies)
        {
            var enemy = collider.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, aoeDamage);
                enemiesHit++;
            }
        }
        
        Debug.Log($"<color=yellow>⭐ CRITICAL 20! AOE dealt {aoeDamage} damage to {enemiesHit} enemies!</color>");
        
        // Optional: Visual effect for AOE
        CreateAOEVisual(center);
    }

    private void CreateAOEVisual(Vector3 center)
    {
        // Create a temporary sphere to visualize AOE
        GameObject aoeVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aoeVisual.transform.position = center;
        aoeVisual.transform.localScale = Vector3.one * aoeSize * 2f;
        
        // Make it transparent yellow
        var renderer = aoeVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 1f, 0f, 0.3f);
            renderer.material.SetFloat("_Mode", 3); // Transparent mode
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
        
        // Remove collider
        Destroy(aoeVisual.GetComponent<Collider>());
        
        // Destroy after 0.3 seconds
        Destroy(aoeVisual, 0.3f);
    }

    private void ReturnToPool()
    {
        // Clean up text before returning to pool
        if (textObj != null)
        {
            Destroy(textObj);
            textObj = null;
            numberText = null;
        }
        
        // Return to pool instead of Destroy
        PoolManager.Despawn(gameObject, PoolManager.PoolType.GameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = rolledNumber == 20 ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw potential AOE range if rolled 20
        if (rolledNumber == 20)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, aoeSize);
        }
    }
}