using Unity.Netcode;
using UnityEngine;

public class WallHealth : NetworkBehaviour
{
    public int maxHits = 15;
    private int currentHits = 0;

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        currentHits += amount;
        Debug.Log($"[Server] Wall hit {currentHits}/{maxHits}");

        if (currentHits >= maxHits)
        {
            Debug.Log("[Server] Wall destroyed!");
            HideWallClientRpc();
        }
    }

    [ClientRpc]
    private void HideWallClientRpc()
    {
        gameObject.SetActive(false); // Ẩn trên cả client và host
    }

}
