
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    // Thời gian update path để giảm CPU load
    private float pathUpdateInterval = 0.3f;
    private float pathTimer;

    public void Setup(Transform playerTarget)
    {
        player = playerTarget;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
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

    public void TakeDamage(float amount)
    {
        Debug.Log("Enemy Take Damage: " + amount);
    }
}
