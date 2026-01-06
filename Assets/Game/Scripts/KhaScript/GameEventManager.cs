using UnityEngine;
using System;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance {get; private set;}

    public event EventHandler<OnChestInteractEventArgs> OnChestInteracted;
    public class OnChestInteractEventArgs
    {
        public Item item { get; private set; }
        public OnChestInteractEventArgs(Item item)
        {
            this.item = item;
        }
    }
    

    // Giả định bạn có một BossManager để thực hiện việc spawn thực tế
    // Nếu bạn muốn xử lý spawn Boss ngay trong GameEventManager, bạn có thể bỏ qua bước này.
    private BossManager bossManager;

    // Điều kiện Spawn Boss: Hàm trả về bool (true nếu Boss có thể spawn)
    // Giá trị này có thể dễ dàng thay đổi từ bất kỳ đâu (ví dụ: một hệ thống quest)
    public Func<bool> CanSpawnBoss { get; set; } = () => true;

    // (Thêm Boss Prefab và các Manager khác tại đây)

    private void Awake()
    {
        if (Instance != null){
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Khởi tạo các Handler ở đây
        gameObject.AddComponent<ChestEventHandler>();
    }
    // HÀM KHỞI TẠO MỚI: Nhận BossManager từ GameManager
    //public void Initialize(BossManager bossManagerInstance)
    //{
    //    Debug.Log("<color=red> [BossManager] spawned boss manager");
    //    this.bossManager = bossManagerInstance;
    //    // Khởi tạo các điều kiện mặc định sau khi đã có tham chiếu
    //    CanSpawnBoss = CheckDefaultSpawnCondition;

    //    if (this.bossManager == null)
    //    {
    //        Debug.LogError("[GameEventManager] BossManager instance bị thiếu khi khởi tạo.");
    //    }
    //}

    //void OnEnable()
    //{
    //    BossGateTrigger.OnBossGateInteracted += HandleBossGateInteraction;
    //}

    //void OnDisable()
    //{
    //    BossGateTrigger.OnBossGateInteracted -= HandleBossGateInteraction;
    //}

    //private void HandleBossGateInteraction(Transform spawnPoint)
    //{
    //    Debug.Log("<color=purple>[GameEventManager]</color> Đã nhận sự kiện tương tác Boss Gate.");

    //    // KIỂM TRA ĐIỀU KIỆN SPAWN BOSS (Có thể dễ dàng thay đổi Func này)
    //    if (CanSpawnBoss.Invoke())
    //    {
    //        Debug.Log("<color=green>[GameEventManager]</color> Điều kiện hợp lệ. Bắt đầu Spawn Boss...");
    //        // Gọi hàm Spawn Boss (thuộc BossManager)
    //        if (bossManager != null)
    //        {
    //            Debug.Log("<color=red> SpawnBoss");
    //            bossManager.SpawnBoss(spawnPoint);
    //        }
    //        else
    //        {
    //            Debug.LogError("[GameEventManager] Thiếu BossManager.");
    //        }

    //        // Ngừng lắng nghe sự kiện sau khi Boss được spawn (tránh spawn nhiều lần)
    //        BossGateTrigger.OnBossGateInteracted -= HandleBossGateInteraction;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("<color=red>[GameEventManager]</color> Điều kiện Spawn Boss chưa hợp lệ!");
    //        // Thêm logic hiển thị UI báo lỗi hoặc tin nhắn cho Player
    //    }
    //}

    //public void HandleChestInteraction(ChestTrigger chest)
    //{
    //    Debug.Log($"<color=teal>[GameEventManager]</color> Player tương tác với Chest loại: {chest.type}.");

    //    switch (chest.type)
    //    {
    //        case ChestTrigger.ChestType.Normal:
    //            // Gọi ItemManager để tạo Item ngẫu nhiên
    //            // ItemManager.DropRandomLoot(chest.transform.position); 
    //            //Debug.Log("<color=teal>[GameEventManager]</color> Thả vật phẩm ngẫu nhiên từ Chest Normal.");
    //            break;
    //        case ChestTrigger.ChestType.KeyRequired:
    //            // Logic kiểm tra xem Player có Key không
    //            // if (PlayerInventory.HasKey) { ItemManager.DropRareLoot(...); }
    //            break;
    //            // ...
    //    }
    //    // Vô hiệu hóa đối tượng Chest sau khi mở
    //    Destroy(chest.gameObject);
    //}

    public void OnChestOpened(Item item)
    {
        OnChestInteracted?.Invoke(this, new OnChestInteractEventArgs(item));
        Debug.Log("<color=cyan>[GameEventManager]</color> Sự kiện OnChestOpened đã được kích hoạt.");
    }

    // Hàm điều kiện spawn mặc định (chỉ là ví dụ)
    private bool CheckDefaultSpawnCondition()
    {
        // Ví dụ: Kiểm tra xem player có đủ level không, hoặc không có Boss nào đang hoạt động
        return true;
    }



    //private void OnEnable()
    //{
    //    GameEventSystem.OnInteract += HandleInteraction;
    //}

    //private void OnDisable()
    //{
    //    GameEventSystem.OnInteract -= HandleInteraction;
    //}

    private void HandleInteraction(object sender, InteractionEventArgs e)
    {
        switch (e.InteractionType)
        {
            case InteractionType.Chest:
                //ChestLogic call
                GameplayEvents.RaiseChestInteracted(e.Target);
                Debug.Log($"Mở Chestzxcvzxcvzxczxcv: {e.Target.name}");
                break;

            case InteractionType.Event:
                //ChestLogic call
                GameplayEvents.RaiseEventInteracted(e.Target);
                Debug.Log($"Trigger Event: {e.Target.name}");
                break;

            case InteractionType.BossGate:
                //ChestLogic call
                GameplayEvents.RaiseBossGateInteracted(e.Target);
                Debug.Log($"Trigger Boss Gate Event: {e.Target.name}");
                break;
        }
    }
}