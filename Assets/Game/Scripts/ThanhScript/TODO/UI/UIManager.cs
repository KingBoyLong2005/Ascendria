using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private HealthBarUI healthBarUI;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;

        healthBarUI = FindFirstObjectByType<HealthBarUI>();
    }

    private void PlayerStatManager_OnPlayerHealthChange(object sender, PlayerStatManager.OnPlayerHealthChangeEventArgs e)
    {
        healthBarUI.SetHealth(e.currentHealth, e.maxHealth);
    }
}
