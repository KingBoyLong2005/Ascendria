
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyStats stats;
    private Transform player;

    // Thời gian update path để giảm CPU load
    private float pathUpdateInterval = 0.3f;
    private float pathTimer;

    public void Setup(Transform playerTarget)
    {
        player = playerTarget;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (stats == null)
            stats = GetComponent<EnemyStats>();

        agent.enabled = true;
        agent.speed = stats.MoveSpeed;

        // Focus vào player
        agent.SetDestination(player.position);

        pathTimer = 0f;
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
    }
}
