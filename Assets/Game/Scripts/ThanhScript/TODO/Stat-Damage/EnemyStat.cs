// EnemyStats.cs
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyData template;  // assign in the editor (for each prefab or spawn logic)

    public string enemyName;
    private float currentHealth;
    private float baseMaxHealth;
    private float moveSpeed;
    private float attack;
    private float armor;

    private float attackCD = 0.5f;
    private float nextAttack = 0f;

    void OnEnable()
    {
        // initialize instance stats from template
        baseMaxHealth = Mathf.Floor(template.maxHealth * EnemyManager.Instance.difficultyMultiplier);
        attack = Mathf.Floor(template.attack * EnemyManager.Instance.difficultyMultiplier);
        armor = Mathf.Floor(template.armor * EnemyManager.Instance.difficultyMultiplier);
        moveSpeed = template.moveSpeed;
        enemyName = template.enemyName;

        currentHealth = baseMaxHealth;
    }
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         Debug.Log($"🎯 {enemyName} ENTERED collision with Player");

    //         // Cho phép đánh ngay lần đầu tiên
    //         if (Time.time >= nextAttack)
    //         {
    //             HitPlayer();
    //             nextAttack = Time.time + attackCD;
    //         }
    //     }
    // }
    //Attack cd for enemy

    private void OnTriggerStay(Collider other)
    {
        if (Time.time >= nextAttack)
        {
            // When this enemy collides with something...
            if (other.gameObject.CompareTag("Player"))
            {
                //Debug.Log("Enemy Hit player");
                HitPlayer();
            }

            // Reset cooldown timer
            nextAttack = Time.time + attackCD;
        }
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        //Debug.Log($"Current enemy hp: {currentHealth}");
        if (currentHealth <= 0f)
            Die();
    }

    public float MaxHealth
    {
        get { return baseMaxHealth; }
    }
    public float Attack
    {
        get { return attack; }
    }
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }
    public float Armor
    {
        get { return armor; }
    }
    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    private void HitPlayer()
    {
        Debug.Log($"🎯 [HIT EVENT] {enemyName} hit player - Sending event to EnemyManager");
        Debug.Log($"   ├─ Enemy GameObject: {this.gameObject.name}");
        Debug.Log($"   ├─ Attack Value: {Attack}");
        Debug.Log($"   └─ Event Time: {Time.time}");
        EnemyManager.Instance.EnemyHitPlayer(this.gameObject, Attack);
    }

    protected virtual void Die()
    {
        //Debug.Log("Chết");
        // Gửi tín hiệu về Manager
        EnemyManager.Instance.EnemyDie(this.gameObject);

        // Despawn (bạn đã comment trong manager → bật lại)
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
    }
}

