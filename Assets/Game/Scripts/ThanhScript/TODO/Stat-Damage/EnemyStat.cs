// EnemyStats.cs
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyData template;  // assign in the editor (for each prefab or spawn logic)

    private float currentHealth;
    private float baseMaxHealth;
    private float moveSpeed;
    private float attack;
    private float armor;

    private float attackCD = 0.5f;
    private float nextAttack = 0f;

    [Header("Testing")]
    private float lifeTime = 5f;
    private float timer;
    public bool DebugTest = true;

    void Awake()
    {
        // initialize instance stats from template
        baseMaxHealth = Mathf.Floor(template.maxHealth * EnemyManager.Instance.difficultyMultiplier);
        attack = Mathf.Floor(template.attack * EnemyManager.Instance.difficultyMultiplier);
        armor = Mathf.Floor(template.armor * EnemyManager.Instance.difficultyMultiplier);
        moveSpeed = template.moveSpeed;

        currentHealth = baseMaxHealth;
        timer = 0f; 
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifeTime && DebugTest)
        {
            Die(); // tự chết sau 5 giây
            timer = 0f;
        }
    }

    //Attack cd for enemy
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
    
    private void HitPlayer()
    {
        EnemyManager.Instance.EnemyHitPlayer(this.gameObject, Attack);
    }

    private void Die()
    {
        Debug.Log("Chết");
        // Gửi tín hiệu về Manager
        EnemyManager.Instance.EnemyDie(this.gameObject);

        // Despawn (bạn đã comment trong manager → bật lại)
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
    }
}

