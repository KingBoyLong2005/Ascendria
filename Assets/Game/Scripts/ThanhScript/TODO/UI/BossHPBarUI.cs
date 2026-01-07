using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBarUI : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI bossName;
    private BossStats boss;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        bossName = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(BossStats bossStats)
    {
        boss = bossStats;
        bossName.text = boss.enemyName;
    }

    private void Update()
    {
        if (boss != null)
        {
            slider.maxValue = boss.MaxHealth;
            slider.value = boss.CurrentHealth;
        }
    }
}

