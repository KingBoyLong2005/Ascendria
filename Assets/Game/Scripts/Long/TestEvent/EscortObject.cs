using UnityEngine;

public class EscortObject : MonoBehaviour
{
    public Transform[] pathPoints;  // đường đi
    public float speed = 2f;
    public float detectionRadius = 5f; // bán kính check người chơi gần

    private int currentPoint = 0;

    public Vector3 halfExtents = new Vector3(7f, 7f, 7f); // kích thước vùng check
    public LayerMask playerLayer;        // layer của Player
    // private Transform player;

    // void Start() {
    //     player = GameObject.FindGameObjectWithTag("Player").transform;
    // }

    // void Update() {
    //     if (player == null) return;

    //     // Chỉ di chuyển nếu player ở gần VÀ nhấn G (test)
    //     if (Vector3.Distance(player.position, transform.position) <= detectionRadius) {
    //         if (Input.GetKey(KeyCode.G)) {   // giữ phím G để di chuyển
    //             MoveAlongPath();
    //         }
    //     }
    // }

    // void MoveAlongPath() {
    //     if (pathPoints.Length == 0) return;

    //     Transform target = pathPoints[currentPoint];
    //     transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

    //     if (Vector3.Distance(transform.position, target.position) < 0.2f) {
    //         currentPoint++;
    //         if (currentPoint >= pathPoints.Length) {
    //             // Đã tới điểm cuối
    //             EventManager.Instance.EndEscortEvent(true);
    //         }
    //     }
    // }

    void Update() {
        
        // Tạo vùng hộp quanh flag và check xem player có trong đó không
        Collider[] hits = Physics.OverlapBox(
            transform.position,   // tâm hộp
            halfExtents,          // half extents
            Quaternion.identity,  // không xoay
            playerLayer           // chỉ check player
        );

        bool inRange = hits.Length > 0;


        // Test: nhấn giữ phím G thì di chuyển
        if (inRange && Input.GetKey(KeyCode.G))
        {
            MoveAlongPath();
        }
    }

    void MoveAlongPath() {
        if (pathPoints.Length == 0) return;

        Transform target = pathPoints[currentPoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f) {
            currentPoint++;
            if (currentPoint >= pathPoints.Length) {
                // Đã tới điểm cuối
                EventManager.Instance.EndEscortEvent(true);
            }
        }
    }
}
