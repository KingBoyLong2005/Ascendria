using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 10f;
    public float gravity = -25f;

    [Header("Surface Check")]
    public float forwardCheckDist = 0.8f;
    public float groundCheckDist = 1.2f;
    public float climbAngleLimit = 75f;

    private Transform player;
    private CharacterController controller;
    private EnemyStats stats;

    private Vector3 velocity;
    private Vector3 surfaceNormal = Vector3.up;
    private bool isClimbing;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // ======= SETUP KHI SPAWN =======
    public void Setup(Transform target)
    {
        player = target;

        if (stats == null)
            stats = GetComponent<EnemyStats>();

        if (stats != null)
            moveSpeed = stats.MoveSpeed;
    }

    void Update()
    {
        if (player == null) return;

        HandleSurface();
        Move();
        ApplyGravity();
    }

    // ======= XỬ LÝ ĐỊA HÌNH / TƯỜNG =======
    void HandleSurface()
    {
        RaycastHit hit;

        // Check phía trước
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, forwardCheckDist))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle > 5f && angle <= climbAngleLimit)
            {
                isClimbing = true;
                surfaceNormal = hit.normal;
                return;
            }
        }

        // Check bên dưới
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, groundCheckDist))
        {
            isClimbing = false;
            surfaceNormal = hit.normal;
            return;
        }

        // Không chạm gì → rơi
        isClimbing = false;
        surfaceNormal = Vector3.up;
    }

    // ======= DI CHUYỂN =======
    void Move()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        // Chiếu hướng di chuyển lên surface
        Vector3 moveDir = Vector3.ProjectOnPlane(dirToPlayer, surfaceNormal).normalized;

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // Xoay mặt
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    // ======= GRAVITY =======
    void ApplyGravity()
    {
        if (isClimbing) 
        {
            velocity.y = 0f;
            return;
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}

// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class EnemyAI : MonoBehaviour
// {
//     [Header("Movement")]
//     public float moveSpeed = 5f;
//     public float rotationSpeed = 5f;
//     public float surfaceCheckDistance = 1f;

//     [Header("Climb / Gravity")]
//     public float gravity = 9.81f;
//     public float stickToSurface = 0.5f;

//     private Transform player;
//     private Rigidbody rb;

//     private EnemyStats stats;

//     public void Setup(Transform target)
//     {
//         player = target;
//         if (stats == null)
//             stats = GetComponent<EnemyStats>();
//         moveSpeed = stats.MoveSpeed;
//     }

//     void Awake()
//     {
//         rb = GetComponent<Rigidbody>();
//     }

//     void FixedUpdate()
//     {
//         if (player == null) return;

//         // 1. Surface detection
//         RaycastHit hit;
//         Vector3 origin = transform.position + Vector3.up * 0.1f;
//         bool onSurface = Physics.Raycast(origin, -transform.up, out hit, surfaceCheckDistance + 0.1f);
//         Vector3 surfaceNormal = onSurface ? hit.normal : Vector3.up;
//         Vector3 surfacePoint = onSurface ? hit.point : transform.position;

//         // 2. Move direction projected on surface
//         Vector3 dirToPlayer = player.position - transform.position;
//         Vector3 moveDir = Vector3.ProjectOnPlane(dirToPlayer, surfaceNormal).normalized;

//         // 3. Apply velocity
//         Vector3 desiredVelocity = moveDir * moveSpeed;
//         rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, 0.2f);

//         // 4. Stick to surface
//         if (onSurface)
//         {
//             Vector3 targetPos = surfacePoint + surfaceNormal * 0.5f;
//             rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * 10f));
//         }
//         else
//         {
//             // rơi khi không có bề mặt
//             rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
//         }

//         // 5. Rotation chỉ theo moveDir
//         if (moveDir != Vector3.zero)
//         {
//             Quaternion targetRot = Quaternion.LookRotation(moveDir, surfaceNormal);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
//         }
//     }

// }
