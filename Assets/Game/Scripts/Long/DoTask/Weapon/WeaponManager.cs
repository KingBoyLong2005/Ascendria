using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance {get; private set;}
    public List<Weapon> weapons ;

    private Weapon wp;
    private PlayerAttack player;

    private bool activated = false;
    bool playerReady = false;
    bool inventoryReady = false;
    // Event Handle 
    public event EventHandler<OnWeaponHitEnemyEventArgs> OnWeaponHitEnemy;
    public class OnWeaponHitEnemyEventArgs : EventArgs
    {
        public GameObject enemy;  // which enemy got hit
        public float weaponAttack;
        public OnWeaponHitEnemyEventArgs(GameObject enemy, float wAtk)
        {
            this.enemy = enemy;
            weaponAttack = wAtk;
        }
    }
    //Event Handle 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    void Start()
    {
        // đăng ký PlayerAttack
        PlayerAttack pa = FindFirstObjectByType<PlayerAttack>();
        if (pa != null)
        {
            pa.OnPlayerAttackReady += HandlePlayerAttackReady;
            Debug.Log("WeaponManager SUB PlayerAttackReady");
            if (pa.IsReady)  // cần thêm property IsReady vào PlayerAttack
            {
                HandlePlayerAttackReady(pa, EventArgs.Empty);
            }
        }

        // đăng ký Inventory
        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            inv.OnInventoryReady += HandleInventoryReady;
            Debug.Log("WeaponManager SUB InventoryReady");
            // 🔥 STICKY READY CHECK
            if (inv.IsReady)
            {
                HandleInventoryReady(inv, EventArgs.Empty);
            }
        }
    }
    private void HandlePlayerAttackReady(object sender, EventArgs e)
    {
        playerReady = true;
        Debug.Log("WeaponManager nhận PlayerReady");
        TryActivate();
    }

    private void HandleInventoryReady(object sender, EventArgs e)
    {
        inventoryReady = true;
        Debug.Log("WeaponManager nhận InventoryReady");
        TryActivate();
    }
    private void TryActivate()
    {
        if (activated) return;
        // {
        //     Debug.Log("activated lỗi");
        //     return;
        // }
        if (!playerReady || !inventoryReady) return;
        // {
        //     Debug.Log("(!playerReady || !inventoryReady) lỗi");
        //     return;
        // }
        

        Debug.Log(">>> WeaponManager TRY ACTIVATE...");

        // kiểm tra inventory đã có weapon chưa
        if (InventoryManager.Instance.activeWeapons.Count == 0)
        {
            Debug.LogWarning("WeaponManager: Inventory chưa có weapon → CHỜ!");
            return;
        }

        // tất cả đủ điều kiện → bắt đầu activate
        player = FindFirstObjectByType<PlayerAttack>();

        weapons = new List<Weapon>();

        foreach (var w in InventoryManager.Instance.activeWeapons)
            weapons.Add(w);

        activated = true;
        Debug.Log(">>> WeaponManager ACTIVATED SUCCESS");
    }
    void Update()
    {
        if (!activated || player == null)
        return;

        Vector3 attackDir = player.GetAttackDirection();
        Vector3 spawnPos = player.ComputeSpawnPosition(attackDir);
        
        WeaponContext ctx = new WeaponContext
        {
            spawnPos = spawnPos,
            forward = attackDir,
            owner = player.transform
        };

        float dt = Time.deltaTime;
        foreach (var w in weapons)
        {
            w.Tick(dt, ctx);
        }
        // if (weapons == null || weapons.Count == 0)
        // {
        //     Debug.LogWarning("WeaponManager: NO WEAPONS FOUND!");
        //     return;
        // }
        wp = weapons.First();
    }
    // --- GET FROM INVENTORY (placeholder) ---
    public Weapon GetFromInventory(Weapon wp)
    {
        // TODO: sau này lấy từ hệ thống Inventory
        return weapons.Find(w => w == wp);
    }
    public void AddWeapon(Weapon w)
    {
        weapons.Add(Instantiate(w));
    }

    // Event handle
    public void WeaponHitEnemy(GameObject enemy, float wAtk)
    {
        OnWeaponHitEnemy?.Invoke(this,new OnWeaponHitEnemyEventArgs(enemy,wAtk));
        //Debug.Log($"Enemy nhận sát thương: {wAtk}");
    }
    // Event handle
}