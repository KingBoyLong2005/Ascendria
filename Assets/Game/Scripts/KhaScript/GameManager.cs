// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class GameManager : MonoBehaviour
// {
//     // Tham chiếu đến các Manager con
//     // Trong kiến trúc Event-Driven, GameManager chỉ cần giữ tham chiếu 
//     // để đảm bảo các Manager con được khởi tạo (có trong Scene)
//     public MapManager mapManager;
//     public PlayerManager playerManager;

//     private void Start()
//     {
//         Debug.Log("--- GameManager: Bắt đầu chuỗi khởi tạo Event-Driven ---");
        
//         // KÍCH HOẠT SỰ KIỆN ĐẦU TIÊN
//         // Hành động này sẽ được MapManager lắng nghe ngay lập tức.
//         GameEvents.TriggerGameStart();
//     }
//     [Header("UI Settings")] // Phân chia rõ ràng trong Inspector
//     public GameObject settingsPanel;
//     private bool isPaused = false;

//     /// <summary>
//     /// Hàm chuyển đổi trạng thái Pause/Resume.
//     /// </summary>
//     public void TogglePause()
//     {
//         isPaused = !isPaused;

//         if (isPaused)
//         {
//             // HIỂN THỊ Panel Settings
//             if (settingsPanel != null)
//             {
//                 settingsPanel.SetActive(true);
//             }
//             // Dừng thời gian game (Pause)
//             Time.timeScale = 0f;
//             Debug.Log("Game Paused.");
//         }
//         else
//         {
//             // ẨN Panel Settings
//             if (settingsPanel != null)
//             {
//                 settingsPanel.SetActive(false);
//             }
//             // Khôi phục thời gian game (Resume)
//             Time.timeScale = 1f;
//             Debug.Log("Game Resumed.");
//         }
//     }

//     /// <summary>
//     /// Trở về Menu chính.
//     /// </summary>
//     public void ReturnToMainMenu()
//     {
//         // Đảm bảo game không bị pause khi chuyển Scene
//         Time.timeScale = 1f; 
//         // Tải lại Scene Menu chính (đã đặt tên là MainMenu)
//         SceneManager.LoadScene("MenuScene"); 
//     }

//     // Thêm một hàm để dễ dàng gọi Pause/Resume qua Input (ví dụ: phím ESC)
//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             TogglePause();
//         }
//     }
// }

using UnityEngine;
using System; // Cần thiết cho Action
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------------------
    // 1. Singleton Pattern (Truy cập GameManager và Events từ mọi nơi)
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ comment nếu muốn nó tồn tại qua các scene
        }
    }
    // -------------------------------------------------------------------

    // -------------------------------------------------------------------
    // 2. Định nghĩa Game Events (Thay thế GameEvents.cs)
    
    // Sự kiện 1: Báo hiệu Game đã bắt đầu và sẵn sàng để tải Map
    public event Action OnGameStart;

    // Sự kiện 2: Báo hiệu Map đã được tải xong và sẵn sàng để spawn Player
    public event Action<List<Transform>> OnMapLoaded;
    // Các sự kiện khác sẽ được thêm vào đây (ví dụ: OnEnemyKilled, OnGameOver, etc.)
    // SỰ KIỆN MỚI: Báo hiệu Map và Player đã sẵn sàng
    public event Action OnMapAndPlayerReady;


    // --- Dữ liệu Public để thiết lập trong Inspector ---
    [Header("Map Setup")]
    public GameObject mapPrefab; 
    public Transform mapSpawnPoint; 

    [Header("Player Setup")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    [Header("Boss Setup")] // THÊM DỮ LIỆU BOSS PREFAB
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    // --- Tham chiếu đến các Manager con (là C# Class) ---
    private MapManager01 mapManager;
    private PlayerManager01 playerManager;
    private BossManager bossManager; // THÊM BIẾN NÀY

    private void Start()
    {
        // 1. XỬ LÝ VỊ TRÍ MẶC ĐỊNH (DEFAULT SPAWN POINTS)
        // Nếu người dùng không gán Transform nào trong Inspector, tạo một đối tượng tạm thời

        Transform actualMapSpawnPoint = GetDefaultTransform(mapSpawnPoint, Vector3.zero, "Map Spawn Default");
        Transform actualPlayerSpawnPoint = GetDefaultTransform(playerSpawnPoint, new Vector3(0, 30, 0), "Player Spawn Default");

        // 2. KHỞI TẠO CÁC MANAGER CON
        if (actualMapSpawnPoint != null && actualPlayerSpawnPoint != null)
        {
            mapManager = new MapManager01(mapPrefab, actualMapSpawnPoint);
            playerManager = new PlayerManager01(playerPrefab, actualPlayerSpawnPoint);
        }
        else
        {
            Debug.LogError("Không thể tạo các điểm spawn mặc định. Game không thể khởi động!");
            return;
        }

        // KHỞI TẠO BOSS MANAGER
        bossManager = new BossManager(bossPrefab, bossSpawnPoint); // THÊM BOSS MANAGER

        // 3. KÍCH HOẠT SỰ KIỆN ĐẦU TIÊN
        TriggerGameStart();
    }

    // Hàm phụ trợ để kiểm tra và tạo Transform mặc định
    private Transform GetDefaultTransform(Transform currentTransform, Vector3 defaultPosition, string debugName)
    {
        if (currentTransform != null)
        {
            return currentTransform; // Đã có Transform được gán từ Inspector
        }

        // Nếu null, tạo một GameObject tạm thời chỉ để giữ vị trí Transform
        GameObject defaultObj = new GameObject($"[{debugName}] Default");
        defaultObj.transform.position = defaultPosition;

        // Đặt đối tượng này là con của GameManager để dễ quản lý trong Hierarchy
        defaultObj.transform.SetParent(this.transform);

        Debug.Log($"<color=orange>Sử dụng vị trí mặc định ({defaultPosition}) cho {debugName}</color>.");
        return defaultObj.transform;
    }

    private void OnDestroy()
    {
        // Dọn dẹp Manager con
        if (mapManager != null) mapManager.Dispose();
        if (playerManager != null) playerManager.Dispose();
        if (bossManager != null) bossManager.Dispose(); // THÊM DISPOSE
    }
    
    // -------------------------------------------------------------------
    // 3. Hàm Kích hoạt Sự kiện (Trigger Functions)
    
    public void TriggerGameStart()
    {
        OnGameStart?.Invoke();
        Debug.Log("<color=green>[GM Triggered]</color> Game Start Event.");
    }

    // Hàm này sẽ được gọi từ MapManager để báo hiệu Map đã tải xong
    public void TriggerMapLoaded(List<Transform> spawnPoints)
    {
        OnMapLoaded?.Invoke(spawnPoints);
        Debug.Log("<color=green>[GM Triggered]</color> Map Loaded Event (w/ Spawn Points).");
    }
    public void TriggerMapAndPlayerReady()
    {
        OnMapAndPlayerReady?.Invoke();
        Debug.Log("<color=red>[GM Triggered]</color> Map & Player Ready Event.");
    }
}