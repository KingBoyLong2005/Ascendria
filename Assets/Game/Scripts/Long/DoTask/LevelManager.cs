using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Progression")]
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNext = 100f;
    public float growthFactor = 1.5f;

    // Event:
    // - OnCreated: bắn khi Instance đã được gán xong (UI dùng để đăng ký OnLevelUp)
    // - OnLevelUp: gửi mỗi khi lên level
    public static event EventHandler OnCreated;
    public event EventHandler<int> OnLevelUp;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Báo cho LevelUpUI biết rằng LevelManager đã sẵn sàng
        OnCreated?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        // TEST nâng cấp bằng phím U 
        // (Bạn có thể xoá nếu không cần)
        if (Input.GetKeyDown(KeyCode.U))
            AddXP(100);
    }

    /// <summary>
    /// Thêm XP và xử lý logic lên cấp
    /// </summary>
    public void AddXP(float amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;

            // tăng mức XP cần cho level tiếp theo
            xpToNext *= growthFactor;

            // báo UI rằng đã lên level mới
            OnLevelUp?.Invoke(this, level);
        }
    }

    /// <summary>
    /// Lấy tỉ lệ % XP để hiện UI
    /// </summary>
    public float GetProgress01()
    {
        if (xpToNext <= 0) return 0f;
        return Mathf.Clamp01(currentXP / xpToNext);
    }
}
