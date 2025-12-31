using UnityEngine;

public class EnemyAI: MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotateSpeed = 10f;
    public float surfaceCheckDist = 1.2f;

    private Transform player;
    private Vector3 currentUp = Vector3.up;
    private EnemyStats stats;

    public void Setup(Transform target)
    {
        player = target;

        if (stats == null)
            stats = GetComponent<EnemyStats>();
        moveSpeed = stats.MoveSpeed;
    }

    void Update()
    {
        if (!player) return;

        // Hướng về player
        Vector3 toPlayer = (player.position - transform.position).normalized;

        // Raycast tìm bề mặt phía trước
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, surfaceCheckDist))
        {
            currentUp = hit.normal;
        }

        // Align rotation theo surface
        Quaternion targetRot = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(toPlayer, currentUp),
            currentUp
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );

        // Di chuyển
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
