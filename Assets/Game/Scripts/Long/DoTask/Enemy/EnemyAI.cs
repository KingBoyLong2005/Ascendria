using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Surface Detection")]
    public float groundCheckDistance = 1f;
    public float wallCheckDistance = 0.8f;
    public float surfaceStickForce = 10f;
    
    [Header("Surface Angles")]
    [Tooltip("Góc nhỏ hơn = Ground, lớn hơn = Wall")]
    public float groundAngleThreshold = 45f;
    [Tooltip("Góc lớn hơn để coi là tường thẳng đứng")]
    public float wallAngleThreshold = 60f;

    [Header("Climb")]
    public float climbSpeed = 3f;

    private enum SurfaceState
    {
        Ground,
        Wall,
        Air
    }

    private Transform player;
    private Rigidbody rb;
    private EnemyStats stats;

    private SurfaceState currentState = SurfaceState.Air;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private Vector3 lastValidSurface = Vector3.up;
    private bool isStuckToSurface = false;

    public void Setup(Transform target)
    {
        player = target;
        if (stats == null)
            stats = GetComponent<EnemyStats>();
        if (stats != null)
            moveSpeed = stats.MoveSpeed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        DetectSurface();
        MoveTowardsPlayer();
        AlignToSurface();
    }

    private void DetectSurface()
    {
        Vector3 bodyCenter = transform.position + Vector3.up * 0.5f;
        bool foundSurface = false;

        // 1. Check ground trước (raycast xuống dưới)
        if (Physics.Raycast(bodyCenter, Vector3.down, out RaycastHit groundHit, groundCheckDistance))
        {
            float angle = Vector3.Angle(groundHit.normal, Vector3.up);
            
            if (angle < groundAngleThreshold)
            {
                currentState = SurfaceState.Ground;
                currentSurfaceNormal = groundHit.normal;
                lastValidSurface = currentSurfaceNormal;
                isStuckToSurface = true;
                return;
            }
        }

        // 2. Check tường (8 hướng để phủ kín)
        Vector3[] directions = {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(bodyCenter, dir, out RaycastHit wallHit, wallCheckDistance))
            {
                float angle = Vector3.Angle(wallHit.normal, Vector3.up);
                
                if (angle > wallAngleThreshold)
                {
                    currentState = SurfaceState.Wall;
                    currentSurfaceNormal = wallHit.normal;
                    lastValidSurface = currentSurfaceNormal;
                    isStuckToSurface = true;
                    foundSurface = true;
                    break;
                }
            }
        }

        if (foundSurface) return;

        // 3. Check hướng player (aggressive wall detection)
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        
        if (Physics.Raycast(bodyCenter, dirToPlayer, out RaycastHit forwardHit, wallCheckDistance * 2f))
        {
            float angle = Vector3.Angle(forwardHit.normal, Vector3.up);
            
            if (angle > wallAngleThreshold)
            {
                currentState = SurfaceState.Wall;
                currentSurfaceNormal = forwardHit.normal;
                lastValidSurface = currentSurfaceNormal;
                isStuckToSurface = true;
                return;
            }
        }

        // 4. Spherecast để phát hiện surface gần
        if (Physics.SphereCast(bodyCenter, 0.3f, Vector3.down, out RaycastHit sphereHit, groundCheckDistance))
        {
            float angle = Vector3.Angle(sphereHit.normal, Vector3.up);
            
            if (angle < groundAngleThreshold)
            {
                currentState = SurfaceState.Ground;
                currentSurfaceNormal = sphereHit.normal;
            }
            else if (angle > wallAngleThreshold)
            {
                currentState = SurfaceState.Wall;
                currentSurfaceNormal = sphereHit.normal;
            }
            
            lastValidSurface = currentSurfaceNormal;
            isStuckToSurface = true;
            return;
        }

        // 5. Không tìm thấy surface
        currentState = SurfaceState.Air;
        currentSurfaceNormal = lastValidSurface;
        isStuckToSurface = false;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Vector3 moveDir;
        float speed;

        switch (currentState)
        {
            case SurfaceState.Ground:
                // Di chuyển bình thường trên mặt đất
                moveDir = Vector3.ProjectOnPlane(dirToPlayer, currentSurfaceNormal).normalized;
                speed = moveSpeed;
                
                rb.linearVelocity = moveDir * speed;
                
                // Nhẹ nhàng đẩy xuống ground
                if (isStuckToSurface)
                {
                    rb.AddForce(-currentSurfaceNormal * surfaceStickForce * 0.3f, ForceMode.Acceleration);
                }
                break;

            case SurfaceState.Wall:
                // Leo tường
                Vector3 climbUp = Vector3.ProjectOnPlane(Vector3.up, currentSurfaceNormal).normalized;
                Vector3 climbRight = Vector3.Cross(currentSurfaceNormal, climbUp).normalized;

                // Kiểm tra nếu vector không hợp lệ
                if (climbUp == Vector3.zero || climbRight == Vector3.zero)
                {
                    // Fallback: di chuyển song song với tường
                    moveDir = Vector3.ProjectOnPlane(dirToPlayer, currentSurfaceNormal).normalized;
                }
                else
                {
                    // Phân tích hướng player theo trục tường
                    float upAmount = Vector3.Dot(dirToPlayer, climbUp);
                    float rightAmount = Vector3.Dot(dirToPlayer, climbRight);

                    moveDir = (climbUp * upAmount + climbRight * rightAmount).normalized;
                }

                speed = climbSpeed;
                rb.linearVelocity = moveDir * speed;
                
                // Lực bám vào tường (mạnh hơn)
                rb.AddForce(-currentSurfaceNormal * surfaceStickForce, ForceMode.Acceleration);
                break;

            case SurfaceState.Air:
                // Rơi và di chuyển về phía player
                moveDir = Vector3.ProjectOnPlane(dirToPlayer, Vector3.up).normalized;
                Vector3 horizontalVel = moveDir * moveSpeed * 0.5f;
                
                rb.linearVelocity = new Vector3(
                    horizontalVel.x,
                    rb.linearVelocity.y - 9.81f * Time.fixedDeltaTime,
                    horizontalVel.z
                );
                break;
        }
    }

    private void AlignToSurface()
    {
        Vector3 forward;
        Vector3 up;

        switch (currentState)
        {
            case SurfaceState.Ground:
                // Đứng vuông góc với ground, nhìn về player
                forward = Vector3.ProjectOnPlane(player.position - transform.position, currentSurfaceNormal).normalized;
                up = currentSurfaceNormal;
                
                if (forward != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(forward, up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
                }
                break;

            case SurfaceState.Wall:
                // Bám vào tường, đầu hướng lên
                Vector3 climbUp = Vector3.ProjectOnPlane(Vector3.up, currentSurfaceNormal).normalized;
                
                if (climbUp != Vector3.zero && currentSurfaceNormal != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(-currentSurfaceNormal, climbUp);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
                }
                break;

            case SurfaceState.Air:
                // Giữ hướng về player, đứng thẳng
                forward = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up).normalized;
                
                if (forward != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
                }
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 bodyCenter = transform.position + Vector3.up * 0.5f;

        // Ground check
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(bodyCenter, bodyCenter + Vector3.down * groundCheckDistance);

        // Wall check rays
        Gizmos.color = Color.red;
        Vector3[] dirs = { 
            transform.forward, -transform.forward, 
            transform.right, -transform.right 
        };
        
        foreach (Vector3 dir in dirs)
        {
            Gizmos.DrawLine(bodyCenter, bodyCenter + dir * wallCheckDistance);
        }

        // Current state indicator
        Gizmos.color = currentState == SurfaceState.Ground ? Color.green :
                       currentState == SurfaceState.Wall ? Color.blue :
                       Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Surface normal
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + currentSurfaceNormal * 1.5f);

        // State label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"State: {currentState}");
        #endif
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
