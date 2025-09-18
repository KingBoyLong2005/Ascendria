using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Flag : NetworkBehaviour
{
    public float captureFlagProgress = 0f;
    public float captureFlagNeeded = 5f;
    public float decayRateFlag = 1f;
    public float detectionRadius = 5f;

    private SphereCollider detectionCollider;

    private NetworkVariable<ulong> controllingPlayer = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isCapturing = false;

    // Danh sách người chơi hiện đang trong vùng
    private List<ulong> playersInZone = new List<ulong>();

    private void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ShowCaptureBarClientRpc(captureFlagNeeded);
        }
    }

    void Update()
    {

        // --- Client side: input ---
        if (IsClient && controllingPlayer.Value == NetworkManager.Singleton.LocalClientId)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log($"[CLIENT] Player {NetworkManager.Singleton.LocalClientId} ấn F để capture");
                StartCaptureServerRpc();
            }
        }

        if (!IsServer) return;

        if (detectionCollider.radius != detectionRadius)
            detectionCollider.radius = detectionRadius;

        // Nếu có player đang capture
        if (isCapturing && controllingPlayer.Value != ulong.MaxValue)
        {
            captureFlagProgress += Time.deltaTime * 2f;
        }
        else
        {
            // Không ai capture thì giảm dần
            captureFlagProgress -= decayRateFlag * Time.deltaTime;
            if (captureFlagProgress < 0) captureFlagProgress = 0;
        }

        // Cập nhật UI cho tất cả client
        UpdateCaptureBarClientRpc(captureFlagProgress);

        if (captureFlagProgress >= captureFlagNeeded)
        {
            Debug.Log("[Flag] Capture hoàn tất! Event thành công.");
            EventManager.Instance.EndEvent(true);
            HideCaptureBarClientRpc();
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCaptureServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        if (sender == controllingPlayer.Value)
        {
            Debug.Log($"[SERVER] Player {sender} bắt đầu capture");
            isCapturing = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null) return;
        
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

        // Nếu người rời đi chính là controllingPlayer
        if (playerId == controllingPlayer.Value)
        {
            Debug.Log($"[SERVER] Player {playerId} rời vùng -> mất quyền");
            isCapturing = false;

            if (playersInZone.Count > 0)
            {
                // Gán quyền cho người đầu tiên trong list (người vào sớm nhất còn lại)
                controllingPlayer.Value = playersInZone[0];
                Debug.Log($"[SERVER] Chuyển quyền cho player {controllingPlayer.Value}");
            }
            else
            {
                controllingPlayer.Value = ulong.MaxValue;
                Debug.Log("[SERVER] Không còn ai trong vùng");
            }
        }
    }

    // --- UI đồng bộ ---
    [ClientRpc]
    private void ShowCaptureBarClientRpc(float maxValue)
    {
        UIManager.Instance.ShowCaptureBar(maxValue);
    }

    [ClientRpc]
    private void UpdateCaptureBarClientRpc(float progress)
    {
        UIManager.Instance.UpdateCapturebar(progress);
    }

    [ClientRpc]
    private void HideCaptureBarClientRpc()
    {
        UIManager.Instance.HideCaptureBar();
    }
}
