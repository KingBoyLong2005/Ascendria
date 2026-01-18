using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    public Transform target;

    [Header("Movement")]
    public float chaseSpeed = 4f;
    public float climbSpeed = 3f;
    public float gravity = -20f;
    public float rotateSpeed = 10f;

    [Header("Detection")]
    public float wallCheckDistance = 1.2f;
    public float heightThreshold = 1.2f;
    public LayerMask climbableLayer;

    private CharacterController controller;
    private float verticalVelocity;

    private enum State
    {
        Chase,
        Climb
    }

    private State currentState = State.Chase;

    public void Setup(Transform player)
    {
        target = player;
    }
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        target = PlayerMoveManager.Instance.transform;
    }

    void Update()
    {
        if (target == null) return;

        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;

            case State.Climb:
                Climb();
                break;
        }
    }

    // ================= CHASE =================
    void Chase()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        dir.Normalize();

        // Rotate toward player
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                rotateSpeed * Time.deltaTime
            );
        }

        // Horizontal movement
        controller.Move(dir * chaseSpeed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Switch to climb
        if (PlayerIsAbove() && CheckWall())
        {
            verticalVelocity = 0f;
            currentState = State.Climb;
        }
    }

    // ================= CLIMB =================
    void Climb()
    {
        // Move straight up
        controller.Move(Vector3.up * climbSpeed * Time.deltaTime);

        // Still face the wall
        if (CheckWall(out RaycastHit hit))
        {
            Quaternion wallRot = Quaternion.LookRotation(-hit.normal);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                wallRot,
                rotateSpeed * Time.deltaTime
            );
        }
        else
        {
            // Reached top → back to chase
            currentState = State.Chase;
        }
    }

    // ================= HELPERS =================
    bool PlayerIsAbove()
    {
        return target.position.y > transform.position.y + heightThreshold;
    }

    bool CheckWall()
    {
        return Physics.Raycast(
            transform.position,
            transform.forward,
            wallCheckDistance,
            climbableLayer
        );
    }

    bool CheckWall(out RaycastHit hit)
    {
        return Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            wallCheckDistance,
            climbableLayer
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            transform.position,
            transform.forward * wallCheckDistance
        );
    }
#endif
}
