using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatRewardPopup : MonoBehaviour
{
    public static StatRewardPopup Instance { get; private set; }

    [Header("UI References - Kéo thả từ Inspector")]
    public GameObject popupPanel;           // Panel chính (bắt đầu inactive)
    // public TextMeshProUGUI titleText;       // Text tiêu đề (ví dụ: "Chọn 1 chỉ số thưởng")
    
    [Header("3 Option Slots")]
    public Button[] optionButtons = new Button[3];
    public TextMeshProUGUI[] statNameTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] statBoostTexts = new TextMeshProUGUI[3]; // Hiển thị "+X" và tên stat

    [Header("Cài đặt Reward")]
    public float boostAmount = 5f;          // Số lượng tăng (có thể chỉnh trong Inspector)

    private List<StatType> selectedStats = new List<StatType>();

    private enum StatType
    {
        MaxHealth,
        Attack,
        MoveSpeed,
        Armor,
        Damage,
        Luck,
        Wealth,
        Wise,
        Coin,
        Discard,
        Silver
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        popupPanel.SetActive(false);
    }

    /// <summary>
    /// Gọi hàm này sau khi hoàn thành 30s đứng trong sphere (ở script Interactable của bạn)
    /// Ví dụ: StatRewardPopup.Instance.ShowRewardPopup();
    /// </summary>
    public void ShowRewardPopup()
    {
        popupPanel.SetActive(true);
        GameManager.Instance.PauseGame();
        FindFirstObjectByType<TPCameraController>().TurnOnMouse();
        GenerateRandomStats();

        for (int i = 0; i < 3; i++)
        {
            StatType stat = selectedStats[i];
            string name = GetStatDisplayName(stat);
            float currentValue = GetCurrentStatValue(stat);

            Color statColor = statColors.TryGetValue(stat, out Color c) ? c : Color.white;

            statNameTexts[i].text  = name;
            statNameTexts[i].color = statColor;  // ← màu tên stat

            statBoostTexts[i].text  = $"+{boostAmount} → {currentValue + boostAmount:F1}";
            statBoostTexts[i].color = statColor;  // ← màu boost text cùng màu

            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionChosen(index));
        }

        // titleText.text = "CHỌN 1 CHỈ SỐ THƯỞNG";
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
        ShowRewardPopup();
        }
    }
    private void GenerateRandomStats()
    {
        selectedStats.Clear();
        List<StatType> allStats = new List<StatType>((StatType[])Enum.GetValues(typeof(StatType)));

        for (int i = 0; i < 3; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, allStats.Count);
            selectedStats.Add(allStats[randIndex]);
            allStats.RemoveAt(randIndex);
        }
    }

    private string GetStatDisplayName(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return "Max Health";
            case StatType.Attack:    return "Attack";
            case StatType.MoveSpeed: return "Move Speed";
            case StatType.Armor:     return "Armor";
            case StatType.Damage:    return "Damage";
            case StatType.Luck:      return "Luck";
            case StatType.Wealth:    return "Wealth";
            case StatType.Wise:      return "Wisdom";
            case StatType.Coin:      return "Coin";
            case StatType.Discard:   return "Discard";
            case StatType.Silver:    return "Silver";
            default: return "Unknown";
        }
    }

    private float GetCurrentStatValue(StatType type)
    {
        PlayerStatManager ps = PlayerStatManager.Instance;
        switch (type)
        {
            case StatType.MaxHealth: return ps.MaxHealth;
            case StatType.Attack:    return ps.Attack;
            case StatType.MoveSpeed: return ps.MoveSpeed;
            case StatType.Armor:     return ps.Armor;
            case StatType.Damage:    return ps.Damage;
            case StatType.Luck:      return ps.Luck;
            case StatType.Wealth:    return ps.Wealth;
            case StatType.Wise:      return ps.Wise;
            case StatType.Coin:      return ps.Coin;
            case StatType.Discard:   return ps.Discard;
            case StatType.Silver:    return ps.Silver;
            default: return 0f;
        }
    }
    // Thêm vào đầu class, sau các field
    private static readonly Dictionary<StatType, Color> statColors = new Dictionary<StatType, Color>
    {
        { StatType.MaxHealth,  new Color(1f,    0.2f,  0.2f)  },  // Đỏ
        { StatType.Attack,     new Color(1f,    0.5f,  0f)    },  // Cam
        { StatType.MoveSpeed,  new Color(0.2f,  0.6f,  1f)    },  // Xanh dương
        { StatType.Armor,      new Color(0.8f,  0.8f,  0.2f)  },  // Vàng
        { StatType.Damage,     new Color(0.9f,  0.1f,  0.5f)  },  // Hồng đậm
        { StatType.Luck,       new Color(0.2f,  0.9f,  0.3f)  },  // Xanh lá
        { StatType.Wealth,     new Color(1f,    0.85f, 0f)     },  // Vàng gold
        { StatType.Wise,       new Color(0.6f,  0.3f,  1f)    },  // Tím
        { StatType.Coin,       new Color(1f,    0.9f,  0.2f)  },  // Vàng nhạt
        { StatType.Discard,    new Color(0.5f,  0.5f,  0.5f)  },  // Xám
        { StatType.Silver,     new Color(0.8f,  0.9f,  1f)    },  // Bạc
    };
    private void OnOptionChosen(int index)
    {
        StatType chosen = selectedStats[index];
        ApplyStatBoost(chosen);

        popupPanel.SetActive(false);
        GameManager.Instance.ResumeGame();
        FindFirstObjectByType<TPCameraController>().TurnOffMouse();
        // Tùy chọn: thêm hiệu ứng hoặc âm thanh ở đây
        Debug.Log($"[Reward] Đã nhận: +{boostAmount} {GetStatDisplayName(chosen)}");
    }

    private void ApplyStatBoost(StatType type)
    {
        PlayerStatManager ps = PlayerStatManager.Instance;

        switch (type)
        {
            case StatType.MaxHealth: ps.ModifyHealth(boostAmount); break;
            case StatType.Attack:    ps.ModifyAttack(boostAmount); break;
            case StatType.MoveSpeed: ps.ModifyMoveSpeed(boostAmount); break;
            case StatType.Armor:     ps.ModifyArmor(boostAmount); break;
            case StatType.Damage:    ps.ModifyDamage(boostAmount); break;
            case StatType.Luck:      ps.ModifyLuck(boostAmount); break;
            case StatType.Wealth:    ps.ModifyWealth(boostAmount); break;
            case StatType.Wise:      ps.ModifyWise(boostAmount); break;
            case StatType.Coin:      ps.ModifyCoin(boostAmount); break;
            case StatType.Discard:   ps.ModifyDiscard(boostAmount); break;
            case StatType.Silver:    ps.modifySilver(boostAmount); break;
        }
    }

    // Test nhanh trong Editor (nhấn Play rồi gọi từ Context Menu)
    [ContextMenu("Test Show Reward Popup")]
    private void TestShow() => ShowRewardPopup();
}