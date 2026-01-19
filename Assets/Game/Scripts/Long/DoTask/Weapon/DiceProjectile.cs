using UnityEngine;
using TMPro;

public class DiceProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector3 direction;
    private int rolledNumber;
    private LayerMask enemyMask;
    private DiceWeapon sourceWeapon;

    private TextMeshPro numberText;
    private float lifetime = 5f;
    private bool hasHit = false;

    public void Initialize(float baseDmg, float projectileSpeed, Vector3 dir, LayerMask mask, DiceWeapon weapon)
    {
        damage = baseDmg;
        speed = projectileSpeed;
        direction = dir.normalized;
        enemyMask = mask;
        sourceWeapon = weapon;

        // Roll dice (1-6)
        rolledNumber = Random.Range(1, 7);
        
        // Hiển thị số trên xúc xắc
        // CreateNumberDisplay();
        
        // Destroy sau lifetime
        Destroy(gameObject, lifetime);
    }

    private void CreateNumberDisplay()
    {
        // Tạo TextMeshPro để hiển thị số
        GameObject textObj = new GameObject("DiceNumber");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero;
        
        numberText = textObj.AddComponent<TextMeshPro>();
        numberText.text = rolledNumber.ToString();
        numberText.fontSize = 4;
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.color = rolledNumber == 6 ? Color.yellow : Color.white;
        
        // Billboard effect - luôn quay về camera
        textObj.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        
        // Scale text
        textObj.transform.localScale = Vector3.one * 0.5f;
    }

    private void Update()
    {
        // Di chuyển projectile
        transform.position += direction * speed * Time.deltaTime;
        
        // Billboard text để luôn nhìn về camera
        // if (numberText != null && Camera.main != null)
        // {
        //     numberText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        // }
        
        // Quay xúc xắc
        transform.Rotate(Vector3.right * 360f * Time.deltaTime);
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
                // Damage = baseDamage * rolledNumber
                float finalDamage = damage * rolledNumber;
                
                WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, finalDamage);
                
                Debug.Log($"<color=yellow>🎲 Dice rolled {rolledNumber} and dealt {finalDamage} damage to {enemy.name}</color>");
                
                // Nếu tung được 6, tăng crit chance vĩnh viễn
                if (rolledNumber == 6 && sourceWeapon != null)
                {
                    sourceWeapon.OnRolledSix();
                    Debug.Log($"<color=gold>⭐ Rolled a 6! Crit Chance increased!</color>");
                }
                
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}