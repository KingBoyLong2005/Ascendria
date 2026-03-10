using UnityEngine;
using System.Collections.Generic;

public class SpawnPointManager : MonoBehaviour
{
    public enum SpawnType
    {
        Player,     // Dành cho vị trí spawn ngẫu nhiên của người chơi
        BossGate,   // Dành cho vị trí cố định của cổng Boss
        FixedItem   // Dành cho vị trí cố định của Item
                    // Thêm các loại khác (NPC, Shop, v.v.) nếu cần
    }

    // Lưu trữ tất cả các điểm spawn (cả Player, BossGate, FixedItem)
    private Dictionary<SpawnType, List<Transform>> categorizedSpawnPoints = new Dictionary<SpawnType, List<Transform>>();

    private void Awake()
    {
        // Khởi tạo Dictionary
        foreach (SpawnType type in System.Enum.GetValues(typeof(SpawnType)))
        {
            categorizedSpawnPoints[type] = new List<Transform>();
        }

        // Duyệt qua tất cả các con và phân loại chúng
        foreach (Transform child in transform)
        {
            SpawnPointType sp = child.GetComponent<SpawnPointType>();
            if (sp != null)
            {
                categorizedSpawnPoints[sp.type].Add(child);
            }
            else
            {
                Debug.LogWarning($"SpawnPointManager: Con {child.name} không có component SpawnPoint.");
            }
        }
        // ... (Log warning nếu cần)
        Debug.Log("SpawnPointManager: Đã phân loại điểm spawn.");
    }

    /// <summary>
    /// Trả về danh sách điểm spawn đã được lọc theo loại.
    /// </summary>
    public List<Transform> GetSpawnPointsByType(SpawnType type)
    {
        if (categorizedSpawnPoints.ContainsKey(type))
        {
            return categorizedSpawnPoints[type];
        }
        return new List<Transform>(); // Trả về danh sách rỗng nếu không có loại này
    }

    // Giữ hàm cũ để PlayerManager01 vẫn hoạt động
    public List<Transform> GetAvailableSpawnPoints()
    {
        return GetSpawnPointsByType(SpawnType.Player);
    }

}