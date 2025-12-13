using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance {get; private set;}
    public List<Weapon> weapons ;

    private Weapon wp;
    [Header("Spawn positioning")]
    public float spawnOffset = 0.12f;
    public float spawnHeightOffset = 0.0f;
    public LayerMask obstacleMask;

    private PlayerAttack player;
    Camera attackCamera;

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
        // // TÌM player và đăng ký EVENT TRONG AWAKE
        // player = FindFirstObjectByType<PlayerAttack>();
        // if (player != null)
        // {
        //     player.OnPlayerAttackReady += HandlePlayerAttackReady;
        //     Debug.Log("WeaponManager registered PlayerAttackReady in Awake()");
        // }
        // else
        // {
        //     Debug.LogWarning("WeaponManager: PlayerAttack chưa tồn tại trong scene khi Awake()");
        // }
    }

    void Start()
    {
        // đăng ký PlayerAttack
        PlayerAttack pa = FindFirstObjectByType<PlayerAttack>();
        if (pa != null)
        {
            pa.OnPlayerAttackReady += HandlePlayerAttackReady;
            Debug.Log("WeaponManager SUB PlayerAttackReady");
        }

        // đăng ký Inventory
        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            inv.OnInventoryReady += HandleInventoryReady;
            Debug.Log("WeaponManager SUB InventoryReady");
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
        if (!playerReady || !inventoryReady) return;

        Debug.Log(">>> WeaponManager TRY ACTIVATE...");

        // kiểm tra inventory đã có weapon chưa
        if (InventoryManager.Instance.activeWeapons.Count == 0)
        {
            Debug.LogWarning("WeaponManager: Inventory chưa có weapon → CHỜ!");
            return;
        }

        // tất cả đủ điều kiện → bắt đầu activate
        player = FindFirstObjectByType<PlayerAttack>();
        attackCamera = Camera.main;

        weapons = new List<Weapon>();

        foreach (var w in InventoryManager.Instance.activeWeapons)
            weapons.Add(w);

        activated = true;
        Debug.Log(">>> WeaponManager ACTIVATED SUCCESS");
    }
    // void Start()
    // {
    //     player = FindFirstObjectByType<PlayerAttack>();
    //     attackCamera = Camera.main;
    //     // clone tất cả weapon để dùng runtime
    //     weapons = weapons.Select(w => Instantiate(w)).ToList();
    // }
    // private void HandlePlayerAttackReady(object sender, EventArgs e)
    // {
    //     Debug.Log("WeaponManager received PlayerAttackReady");

    //     player = sender as PlayerAttack;
    //     attackCamera = Camera.main;

    //     weapons = new List<Weapon>();

    //     if (player.wp != null)
    //     {
    //         Debug.Log("WeaponManager ADD WEAPON từ PlayerAttack");
    //         weapons.Add(Instantiate(player.wp)); // clone runtime weapon
    //     }
    //     else
    //     {
    //         Debug.LogWarning("PlayerAttack.wp NULL");
    //     }

    //     activated = true;
    //     Debug.Log(">>> WeaponManager ACTIVATED");
    // }

    void Update()
    {
        if (!activated)
            return;
        if (player == null || attackCamera == null)
            return;

        Vector3 forward = player.GetForwardDirection();
        Vector3 spawnPosition = player.ComputeSpawnPosition(forward);
        
        WeaponContext ctx = new WeaponContext
        {
            spawnPos = spawnPosition,
            forward = forward,
            owner = player.transform
        };

        float dt = Time.deltaTime;
        foreach (var w in weapons)
        {
            w.Tick(dt, ctx);
        }
        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogWarning("WeaponManager: NO WEAPONS FOUND!");
            return;
        }
        wp = weapons.First();
        // if(Input.GetKeyDown(KeyCode.U) && DebugTest)
        // {
        //     WeaponUpgrade.Upgrade(wp,RarityHelper.GetRandomRarity());
        //     Debug.Log($"Damage: {wp.damage} \n Range: {wp.range} \n Size: {wp.size} \n Cooldown: {wp.cooldown} ");
        // }
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
    }
    // Event handle
}