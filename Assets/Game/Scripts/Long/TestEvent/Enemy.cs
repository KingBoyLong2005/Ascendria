using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    public NetworkVariable<int> hp = new NetworkVariable<int>(
        1, // default hp
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            hp.Value = 1; // reset máu khi spawn
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int dmg, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        hp.Value -= dmg;
        if (hp.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.AddKill();
        GetComponent<NetworkObject>().Despawn(); // đồng bộ xóa enemy
    }

    // Tạm test bằng click chuột trái
    private void OnMouseDown()
    {
        if (IsOwner || IsClient) // client bắn thì gọi RPC lên server
        {
            TakeDamageServerRpc(1);
        }
    }
}
