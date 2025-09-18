using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class EscortObject : NetworkBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;
    public float detectionRadius = 5f; // sẽ sync với SphereCollider
    public float stayTime = 10f;       // cần đứng trong vùng bao lâu thì di chuyển

    private int currentPoint = 0;
    private NetworkVariable<ulong> controllingPlayer = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    ); // clientId player điều khiển
    private float stayTimer = 0f;
    private bool isCapturing = false;
    private List<ulong> playersInZone = new List<ulong>();


    private SphereCollider detectionCollider;

    private void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
    }

    private void Update()
    {
        if (!IsServer) return;

        // cập nhật bán kính collider khi thay đổi trong Inspector
        if (detectionCollider.radius != detectionRadius)
            detectionCollider.radius = detectionRadius;

        if (controllingPlayer.Value == NetworkManager.Singleton.LocalClientId)
        {
            // tăng timer nếu player vẫn trong vùng
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayTime)
            {
                MoveAlongPath();
                UpdatePositionClientRpc(transform.position, currentPoint);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj != null) return;
        
        ulong playerId = netObj.OwnerClientId;

        if (!playersInZone.Contains(playerId))
            playersInZone.Add(playerId);

        if (controllingPlayer.Value == ulong.MaxValue)
        {
            controllingPlayer.Value = netObj.OwnerClientId;
            Debug.Log($"[SERVER] Player {controllingPlayer.Value} vào vùng -> có thể ấn nút");
        }
        else
        {
            Debug.Log($"[SERVER] Player khác {netObj.OwnerClientId} vào vùng nhưng {controllingPlayer.Value} đang giữ quyền");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null) return;

        ulong playerId = netObj.OwnerClientId;

        if (playersInZone.Contains(playerId))
            playersInZone.Remove(playerId);

        if (playersInZone.Count > 0)
        {
            // Gán quyền cho người đầu tiên trong list (người vào sớm nhất còn lại)
            controllingPlayer.Value = playersInZone[0];
            Debug.Log($"[SERVER] Chuyển quyền cho player {controllingPlayer.Value}");
            stayTimer = 0f;
        }
        else
        {
            controllingPlayer.Value = ulong.MaxValue;
            Debug.Log("[SERVER] Không còn ai trong vùng");
        }
    }

    private void MoveAlongPath()
    {
        if (pathPoints.Length == 0) return;

        Transform target = pathPoints[currentPoint];
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            currentPoint++;
            Debug.Log($"[SERVER] Đạt tới point {currentPoint}");

            if (currentPoint >= pathPoints.Length)
            {
                Debug.Log("[SERVER] EscortObject hoàn thành đường đi -> EndEvent");
                EventManager.Instance.EndEvent(true);
            }
        }
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 pos, int point)
    {
        transform.position = pos;
        currentPoint = point;
    }

    // Debug: vẽ bán kính collision
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
