// EnemyStats.cs
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyData template;  // assign in the editor (for each prefab or spawn logic)

    private float currentHealth;
    private float moveSpeed;
    private float damage;

    private float attackCD = 0.5f;
    private float nextAttack = 0f;

    [Header("Testing")]
    private float lifeTime = 5f;
    private float timer;
    public bool DebugTest = true;

    void Awake()
    {
        // initialize instance stats from template
        currentHealth = template.maxHealth;
        moveSpeed = template.moveSpeed;
        damage = template.damage;
        timer = 0f; 
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifeTime && DebugTest)
        {
            Die(); // tự chết sau 5 giây
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time >= nextAttack)
        {
            // When this enemy collides with something...
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Enemy Hit player");
                HitPlayer();
            }

            // Reset cooldown timer
            nextAttack = Time.time + attackCD;
        }
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    public float GetDamage()
    {
        return damage;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    private void HitPlayer()
    {
        EnemyManager.Instance.EnemyHitPlayer(this.gameObject, template.damage);
    }

    private void Die()
    {
        // Gửi tín hiệu về Manager
        EnemyManager.Instance.EnemyDie(this.gameObject);

        // Despawn (bạn đã comment trong manager → bật lại)
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
    }
}

