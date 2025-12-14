using UnityEngine;
using System;

public class ChestTrigger : MonoBehaviour
{
    // Sự kiện gửi Transform của Chest để biết vị trí mở và loại Chest
    public static event Action<ChestTrigger> OnChestInteracted;

    // Enum để phân loại các loại Chest
    public enum ChestType { Normal, Rare, KeyRequired }
    public ChestType type = ChestType.Normal;

    // ... logic OnTriggerEnter(Collider other)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnChestInteracted?.Invoke(this); // Gửi chính Component này đi
            // Vô hiệu hóa trigger sau khi tương tác (tránh mở nhiều lần)
            // GetComponent<Collider>().enabled = false;
        }
    }
}