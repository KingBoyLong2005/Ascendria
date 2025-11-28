// using UnityEngine;

// public class Enemy : MonoBehaviour
// {
//     private EnemyManager manager;
//     private GameObject prefab;

//     private float lifeTime = 5f;
//     private float timer;
//     public void Setup(EnemyManager m, GameObject p)
//     {
//         manager = m;
//         prefab = p;
//         timer = 0; // reset thời gian khi spawn
//     }
//     private void Update()
//     {
//         timer += Time.deltaTime;

//         if (timer >= lifeTime)
//         {
//             Die(); // tự chết sau 5 giây
//         }
//     }

//     public void Die()
//     {
//         manager.EnemyDie(prefab, gameObject);
//     }
// }
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private EnemyManager manager;
    private GameObject prefab;

    private NavMeshAgent agent;
    private Transform player;

    // Thời gian update path để giảm CPU load
    private float pathUpdateInterval = 0.3f;
    private float pathTimer;

    private float lifeTime = 5f;
    private float timer;

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

        if (timer >= lifeTime)
        {
            Die(); // tự chết sau 5 giây
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
