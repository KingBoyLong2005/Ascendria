using UnityEngine;
using System;

public class ChestTrigger : MonoBehaviour
{
    // Enum để phân loại các loại Chest
    public enum ChestType { Normal, Rare, KeyRequired }
    public ChestType type = ChestType.Normal;

    // ... logic OnTriggerEnter(Collider other)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Item item = ItemManager.Instance.GetRandomItem();
            Debug.Log("<color=cyan>[ChestTrigger]</color> Player đã nhận được vật phẩm: " + item);
            // Lấy list item hiện có -> random 1 item -> trả về item đó ->  player nhận item -> item vào inventory 
            // cần tồn tại 1 list item public -> lấy list đó
            // random 1 item trong list 
            // đưa item vào trong OnChestOpened(item1);
            GameEventManager.Instance.OnChestOpened(item);// -> iventoryManager đăng ký lắng nghe

            Debug.Log("<color=cyan>[ChestTrigger]</color> Player đã tương tác với Chest loại: " + type);
        }
        
    }
    
    // public void Start()
    // {
    //     GameEventManager.Instance.OnChestInteracted += AddItem;

    // }
    /*
    public void HandleChestInteraction(ChestTrigger chest)
    {
        Debug.Log($"<color=teal>[GameEventManager]</color> Player tương tác với Chest loại: {chest.type}.");

        switch (chest.type)
        {
            case ChestTrigger.ChestType.Normal:
                // Gọi ItemManager để tạo Item ngẫu nhiên
                // ItemManager.DropRandomLoot(chest.transform.position); 
                Debug.Log("<color=teal>[GameEventManager]</color> Thả vật phẩm ngẫu nhiên từ Chest Normal.");
                break;
            case ChestTrigger.ChestType.KeyRequired:
                // Logic kiểm tra xem Player có Key không
                // if (PlayerInventory.HasKey) { ItemManager.DropRareLoot(...); }
                break;
                // ...
        }
        // Vô hiệu hóa đối tượng Chest sau khi mở
        Destroy(chest.gameObject);
    }
    */
}