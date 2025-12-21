using UnityEngine;
using System;

public class BossGateTrigger : MonoBehaviour
{
    // Sự kiện được GameEventManager lắng nghe
    public static event Action<Transform> OnBossGateInteracted;

    // Giữ Transform của điểm spawn để GameEventManager biết Boss spawn ở đâu
    private Transform bossSpawnPosition;

    // Hàm khởi tạo để nhận Transform của điểm spawn Boss
    public void Initialize(Transform spawnPoint)
    {
        bossSpawnPosition = spawnPoint;
        // Đảm bảo đối tượng này có Collider và Rigidbody để OnTrigerEnter hoạt động
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError("[BossGateTrigger] Thiếu Collider! Không thể phát hiện Player.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        // Giả định Player có tag "Player"
        if (other.CompareTag("Player"))
        {
            // Chỉ kích hoạt sự kiện nếu có vị trí spawn hợp lệ
            if (bossSpawnPosition != null)
            {
                // 1. Phát tiếng mở cổng
                //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.gateOpenSfx);

                // 2. Đổi sang nhạc Boss
                //AudioManager.Instance.PlayMusic(PrefabDatabase.Instance.bossTheme);
                Debug.Log("<color=red>[BossGateTrigger]</color> Player đã tương tác với Boss Gate.");
                // Kích hoạt sự kiện và truyền vị trí Spawn của Boss Gate đi
                OnBossGateInteracted?.Invoke(bossSpawnPosition);
            }
        }
    }
}