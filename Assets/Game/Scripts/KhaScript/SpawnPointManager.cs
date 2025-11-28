using UnityEngine;
using System.Collections.Generic;

public class SpawnPointManager : MonoBehaviour
{
    // Dùng List để chứa tất cả các Transform của các điểm spawn con
    public List<Transform> playerSpawnPoints = new List<Transform>();

    private void Awake()
    {
        // Tự động tìm tất cả các transform con (children) và thêm chúng vào danh sách
        // Giả định rằng TẤT CẢ các đối tượng con trực tiếp là các điểm spawn

        // Hoặc tìm kiếm theo tag/layer nếu bạn muốn kiểm soát chi tiết hơn

        // Phương pháp đơn giản: Lấy tất cả các con trực tiếp
        foreach (Transform child in transform)
        {
            // Chỉ thêm nếu đó không phải là chính object SpawnPointManager (đối tượng gốc)
            if (child != transform)
            {
                playerSpawnPoints.Add(child);
            }
        }

        if (playerSpawnPoints.Count == 0)
        {
            Debug.LogWarning("SpawnPointManager: Không tìm thấy điểm spawn nào trong object này.");
        }
    }
}