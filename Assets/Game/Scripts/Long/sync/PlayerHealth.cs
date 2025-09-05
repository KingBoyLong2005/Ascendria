using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 20;

    public NetworkVariable<int> Health = new NetworkVariable<int>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Start()
    {

        if (IsServer)
        {
            Health.Value = maxHealth;
        }

        // Đăng ký callback để theo dõi sự thay đổi của Health trên tất cả client và host
        Health.OnValueChanged += OnHealthChanged;
    }

    public void ApplyDamage(int damage, ulong attackerId)
    {
        if (!IsServer) return;
        if (Health.Value <= 0) return;

        Health.Value -= damage;
        Debug.Log($"[Server] Player {OwnerClientId} took {damage} from {attackerId}. HP = {Health.Value}");

        if (Health.Value <= 0)
        {
            Die();
            NotifyDeathClientRpc(OwnerClientId);
        }
    }

    private void Die()
    {
        Debug.Log($"[Server] Player {OwnerClientId} died!");
        // Vô hiệu hóa GameObject trên server
        gameObject.SetActive(false);
        // Gọi ClientRpc để vô hiệu hóa trên tất cả client
        DisablePlayerClientRpc();
    }

    [ClientRpc]
    private void DisablePlayerClientRpc()
    {
        // Vô hiệu hóa GameObject trên client
        gameObject.SetActive(false);
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Player {OwnerClientId} GameObject disabled.");
        // Logic hiển thị trên client: cập nhật UI, thông báo, v.v.
        DisplayDeathNotification(OwnerClientId);
    }

    [ClientRpc]
    private void NotifyDeathClientRpc(ulong clientId)
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Player {clientId} died!");
        // Logic hiển thị trên client: cập nhật UI, thông báo, v.v.
        // Ví dụ: Gọi một phương thức để hiển thị thông báo trên UI
        DisplayDeathNotification(clientId);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Player {OwnerClientId} health changed from {previousValue} to {newValue}");
        // Logic cập nhật UI hoặc hiển thị máu trên client
        // Ví dụ: UpdateHealthUI(newValue);
    }

    private void DisplayDeathNotification(ulong clientId)
    {
        // Thêm logic để hiển thị thông báo trên UI
        // Ví dụ: Hiển thị thông báo "Player X đã chết" trên màn hình
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Displaying death notification for Player {clientId}");
    }

    public override void OnDestroy()
    {
        // Hủy đăng ký callback để tránh memory leak
        Health.OnValueChanged -= OnHealthChanged;
        base.OnDestroy();
    }
}