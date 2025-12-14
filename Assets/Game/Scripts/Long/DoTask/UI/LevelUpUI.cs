using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    public LevelManager levelManager;
    public InventoryManager inventoryManager;
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;
    public int optionCount = 3;

    // [Header("Weapon pool")]
    // public Weapon[] weaponPool;
    // [Header("Buff pool")]
    // public BookBuff[] buffPool;
    [Header("Database")]
    private UpgradeDatabase upgradeDB;


    List<LevelUpOptionUI> spawnedOptions = new();
    bool isShowing = false;

    System.Random rnd = new System.Random();

    EventHandler<int> cachedLevelUpHandler;

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
        levelManager = LevelManager.Instance;
        inventoryManager = FindFirstObjectByType<InventoryManager>();

        // đăng ký sự kiện lên level
        cachedLevelUpHandler = (s, lvl) => OnLevelUp(lvl);
        levelManager.OnLevelUp += cachedLevelUpHandler;

        upgradeDB = UpgradeDatabase.Instance;

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDisable()
    {
        // gỡ event cho sạch
        if (LevelManager.Instance != null && cachedLevelUpHandler != null)
        {
            LevelManager.Instance.OnLevelUp -= cachedLevelUpHandler;
        }
    }

    private void HandleReady(object sender, EventArgs e)
    {
        levelManager = LevelManager.Instance;
        inventoryManager = FindFirstObjectByType<InventoryManager>();

        // tạo 1 delegate duy nhất để unsubscribe được
        cachedLevelUpHandler = (s, lvl) => OnLevelUp(lvl);

        // đăng ký
        levelManager.OnLevelUp += cachedLevelUpHandler;

        if (panel != null) panel.SetActive(false);
    }

    private void OnLevelUp(int newLevel)
    {
        ShowOptions();
    }

    // --- PHẦN BÊN DƯỚI GIỮ NGUYÊN LOGIC CỦA BẠN ---
    // (chỉ sửa giao tiếp event, không đụng đến gameplay của bạn)

    public void ShowOptions()
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = true;

        if (isShowing) return;
        isShowing = true;

        if (panel != null) panel.SetActive(true);

        foreach (var o in spawnedOptions) Destroy(o.gameObject);
        spawnedOptions.Clear();

        // List<Weapon> allWeapons = weaponPool?.Where(x => x != null).ToList() ?? new();
        List<Weapon> allWeapons = upgradeDB.allWeapons.ToList();
        List<Weapon> owned = inventoryManager?.ownedWeapons.Where(x => x != null).ToList() ?? new();
        List<Weapon> unowned = allWeapons.Except(owned).ToList();

        var allBuffTypes = Enum.GetValues(typeof(BuffType)).Cast<BuffType>().ToList();

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
        {
            var inst = Instantiate(optionPrefab, optionsParent);
            inst.Setup(opt, OnUpgradeOptionSelected);
            spawnedOptions.Add(inst);
        }
    }

    void OnUpgradeOptionSelected(UpgradeOption option)
    {
        if (inventoryManager == null)
        {
            CloseOptions();
            return;
        }

        Rarity rarity = (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());

        switch (option.kind)
        {
            // ───────────────────────────────────────────────
            // 1) UPGRADE VŨ KHÍ ĐANG CÓ
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.WeaponUpgrade:
                if (option.targetWeapon != null)
                {
                    // Tìm bản runtime trong inventory (không dùng template)
                    Weapon runtimeWeapon = inventoryManager.ownedWeapons
                        .FirstOrDefault(w => w.weaponName == option.targetWeapon.weaponName);

                    if (runtimeWeapon != null)
                    {
                        // Nâng cấp đúng bản runtime đang dùng
                        WeaponUpgrade.Upgrade(runtimeWeapon, rarity);
                    }
                    else
                    {
                        // Nếu chưa có → thêm mới bằng clone
                        Weapon newRuntime = Instantiate(option.targetWeapon);
                        inventoryManager.AddWeapon(newRuntime);
                        WeaponManager.Instance.AddWeapon(newRuntime);
                    }
                }
                break;


            // ───────────────────────────────────────────────
            // 2) NHẶT VŨ KHÍ MỚI (PHẢI CLONE SCRIPTABLEOBJECT)
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.WeaponDrop:
                if (!inventoryManager.HasWeapon(option.targetWeapon))
                {
                    // Clone thành runtime weapon trước khi thêm
                    Weapon newRuntime = Instantiate(option.targetWeapon);
                    inventoryManager.AddWeapon(newRuntime);
                    WeaponManager.Instance.AddWeapon(newRuntime);
                }
                break;


            // ───────────────────────────────────────────────
            // 3) BUFF (nếu bạn còn dùng)
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.Buff:
            {
                // Kiểm tra nhân vật đã từng có buff này chưa
                BookBuff runtimeBuff = inventoryManager.ownedBookBuffs
                    .FirstOrDefault(b => b.buffId == option.targetBuff.buffId);

                if (runtimeBuff == null)
                {
                    // LẦN ĐẦU NHẬN BUFF → CLONE VÀ LEVELUP 1 LẦN
                    runtimeBuff = Instantiate(option.targetBuff);
                    runtimeBuff.level = 0; // đảm bảo reset template

                    runtimeBuff.LevelUp(rarity);

                    inventoryManager.AddBuff(runtimeBuff);

                    // APPLY VÀO PLAYER
                    BookBuffManager.Instance.ApplyBuff(runtimeBuff);

                    Debug.Log($"Nhận buff mới: {runtimeBuff.name}, level {runtimeBuff.level}");
                }
                else
                {
                    // ĐÃ CÓ → TĂNG LEVEL + APPLY LẠI
                    runtimeBuff.LevelUp(rarity);

                    BookBuffManager.Instance.ApplyBuff(runtimeBuff);

                    Debug.Log($"Buff {runtimeBuff.name} tăng cấp lên {runtimeBuff.level}");
                }
            }
            break;
        }

        CloseOptions();
    }
    UpgradeTier PickRandomTierForDisplay()
    {
        Rarity random = RarityHelper.GetRandomRarity();
        return (UpgradeTier)Enum.Parse(typeof(UpgradeTier), random.ToString());
    }

    public void CloseOptions()
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = false;

        if (!isShowing) return;
        isShowing = false;

        if (panel != null) panel.SetActive(false);
    }

    public enum UpgradeTier { Common, Uncommon, Rare, Epic, Legendary }

    [Serializable]
    public class UpgradeOption
    {
        public enum Kind { WeaponUpgrade, WeaponDrop, Buff }

        public Kind kind;
        public UpgradeTier tier;
        public Weapon targetWeapon;
        public BookBuff targetBuff;
    }

}
