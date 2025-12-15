// LevelUpUI.cs (simplified to only handle UI display; logic moved to LevelManager)
using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;
    public int optionCount = 3; // Not used here anymore, but kept for ref

    List<LevelUpOptionUI> spawnedOptions = new();
    bool isShowing = false;

    EventHandler<LevelManager.LevelUpEventArgs> cachedLevelUpHandler;
    EventHandler<LevelManager.UpgradeSelectedEventArgs> cachedUpgradeSelectedHandler;

    // đảm bảo OnEnable chạy cả trong AddComponent lẫn scene load
    private void OnEnable()
    {
        // Trường hợp 1: LevelManager đã tồn tại trước khi LevelUpUI bật
        if (LevelManager.Instance != null)
        {
            Initialize();
        }
        else
        {
            // Trường hợp 2: LevelManager chưa được AddComponent → chờ OnCreated
            LevelManager.OnCreated += HandleCreated;
        }
    }

    private void HandleCreated(object sender, EventArgs e)
    {
        LevelManager.OnCreated -= HandleCreated; // tránh leak
        Initialize();
    }

    private void Initialize()
    {
        // đăng ký sự kiện lên level
        cachedLevelUpHandler = OnLevelUp;
        LevelManager.Instance.OnLevelUp += cachedLevelUpHandler;

        // Also subscribe to upgrade selected to close UI
        cachedUpgradeSelectedHandler = OnUpgradeSelected;
        LevelManager.Instance.OnUpgradeSelected += cachedUpgradeSelectedHandler;

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDisable()
    {
        // gỡ event cho sạch
        if (LevelManager.Instance != null)
        {
            if (cachedLevelUpHandler != null)
                LevelManager.Instance.OnLevelUp -= cachedLevelUpHandler;
            if (cachedUpgradeSelectedHandler != null)
                LevelManager.Instance.OnUpgradeSelected -= cachedUpgradeSelectedHandler;
        }
    }

    private void OnLevelUp(object sender, LevelManager.LevelUpEventArgs e)
    {
        ShowOptions(e.Options);
    }

    public void ShowOptions(List<LevelManager.UpgradeOption> options)
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = true;

        if (isShowing) return;
        isShowing = true;

        if (panel != null) panel.SetActive(true);

        foreach (var o in spawnedOptions) Destroy(o.gameObject);
        spawnedOptions.Clear();

<<<<<<< Updated upstream
        // List<Weapon> allWeapons = weaponPool?.Where(x => x != null).ToList() ?? new();
        List<Weapon> allWeapons = upgradeDB.allWeapons.ToList();
        List<Weapon> owned = inventoryManager?.ownedWeapons.Where(x => x != null).ToList() ?? new();
        List<Weapon> unowned = allWeapons.Except(owned).ToList();

        List<UpgradeOption> candidates = new();

        foreach (var w in owned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponUpgrade,
                tier = PickRandomTierForDisplay(),
                targetWeapon = w
            });
        }

        foreach (var w in unowned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponDrop,
                tier = UpgradeTier.Common,
                targetWeapon = w
            });
        }

        foreach (var buff in upgradeDB.allBuffs)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.Buff,
                tier = PickRandomTierForDisplay(),
                targetBuff = buff
            });
        }

        candidates = candidates.OrderBy(x => rnd.Next()).ToList();

        List<UpgradeOption> chosen = new();
        foreach (var c in candidates)
        {
            if (chosen.Count >= optionCount) break;

                bool duplicate = chosen.Any(ch =>
                    (c.kind == UpgradeOption.Kind.WeaponUpgrade && ch.targetWeapon == c.targetWeapon) ||
                    (c.kind == UpgradeOption.Kind.WeaponDrop    && ch.targetWeapon == c.targetWeapon) ||
                    (c.kind == UpgradeOption.Kind.Buff          && ch.targetBuff    == c.targetBuff)
                );


            if (!duplicate)
                chosen.Add(c);
        }

        foreach (var opt in chosen)
=======
        foreach (var opt in options)
>>>>>>> Stashed changes
        {
            var inst = Instantiate(optionPrefab, optionsParent);
            inst.Setup(opt);
            inst.OnOptionSelected += OnOptionSelectedHandler; // Subscribe to each option's event
            spawnedOptions.Add(inst);
        }
    }

    private void OnOptionSelectedHandler(object sender, LevelUpOptionUI.LevelUpOptionSelectedEventArgs e)
    {
        // Forward to LevelManager to apply
        LevelManager.Instance.ApplyUpgrade(e.SelectedUpgrade);

        // Unsubscribe from all options to avoid leaks
        foreach (var opt in spawnedOptions)
        {
            opt.OnOptionSelected -= OnOptionSelectedHandler;
        }
    }

    private void OnUpgradeSelected(object sender, LevelManager.UpgradeSelectedEventArgs e)
    {
        CloseOptions();
    }

    public void CloseOptions()
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = false;

        if (!isShowing) return;
        isShowing = false;

        if (panel != null) panel.SetActive(false);
    }
}