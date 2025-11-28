
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private EnemyManager manager;
    private GameObject prefab;

    private NavMeshAgent agent;
    private Transform player;

    // Thời gian update path để giảm CPU load
    private float pathUpdateInterval = 0.3f;
    private float pathTimer;

    public float maxHealth = 10f;
    private float currentHealth;

    [Header("Testing")]
    private float lifeTime = 5f;
    private float timer;
    public bool DebugTest = true;

    public void Setup(EnemyManager m, GameObject p, Transform playerTarget)
    {
        manager = m;
        prefab = p;
        player = playerTarget;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        // Focus vào player
        agent.SetDestination(player.position);

        pathTimer = 0f;
        timer = 0; // reset thời gian khi spawn
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (player != null || agent != null)
        {
            // Cập nhật path mỗi pathUpdateInterval giây
            pathTimer += Time.deltaTime;
            if (pathTimer >= pathUpdateInterval)
            {
                agent.SetDestination(player.position);
                pathTimer = 0f;
            }
        }

        timer += Time.deltaTime;

        if (timer >= lifeTime && DebugTest)
        {
            Die(); // tự chết sau 5 giây
        }
    
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took damage: " + damage + " | HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        // Gửi tín hiệu về Manager
        manager.EnemyDie(prefab, gameObject);

        // Tắt agent trước khi trả về pool
        agent.enabled = false;

        // Despawn (bạn đã comment trong manager → bật lại)
        PoolManager.Instance.Despawn(prefab, gameObject);
    }
}
