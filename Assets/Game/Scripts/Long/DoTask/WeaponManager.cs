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

    [Header("Debug")]
    public bool DebugTest = true;
    private Vector3 debugBoxCenter;
    private Vector3 debugBoxSize;
    private Quaternion debugBoxRot;
    private bool debugDrawBox = false;

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
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
        foreach (var w in weapons)
        {
            if(DebugTest)
            {
                debugBoxCenter = spawnPosition;
                debugBoxSize = new Vector3(w.size, 0.25f, w.range) * 0.5f;
                debugBoxRot = rot;
                debugDrawBox = true;
            }
            else
            {
                debugDrawBox = false;
            }
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
    private void OnDrawGizmos()
    {
        if (!debugDrawBox) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(debugBoxCenter, debugBoxRot, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, debugBoxSize);
    }
    // void Update()
    // {
    //     if (player == null || attackCamera == null)
    //         return;

    //     Vector3 camDir = attackCamera.transform.forward;
    //     Vector3 spawnPos = ComputeSpawnPosition(player.transform, camDir);

    //     WeaponContext ctx = new WeaponContext
    //     {
    //         spawnPos = spawnPos,
    //         forward = new Vector3(camDir.x, 0f, camDir.z).normalized,
    //         owner = player.transform
    //     };

    //     float dt = Time.deltaTime;

    //     foreach (var w in weapons)
    //         w.Tick(dt, ctx);
    // }

    // Vector3 ComputeSpawnPosition(Transform fp, Vector3 dir)
    // {
    //     Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);
    //     if (dirFlat.sqrMagnitude < 0.0001f)
    //         dirFlat = fp.forward;

    //     dirFlat.Normalize();

    //     CapsuleCollider cap = fp.GetComponent<CapsuleCollider>();
    //     float radiusWorld = 0.5f;
    //     float heightWorld = 1.0f;
    //     Vector3 centerWorld = fp.position;

    //     if (cap != null)
    //     {
    //         Vector3 lossy = fp.lossyScale;
    //         float scaleXZ = Mathf.Max(lossy.x, lossy.z);
    //         radiusWorld = cap.radius * scaleXZ;
    //         heightWorld = cap.height * lossy.y;
    //         centerWorld = fp.TransformPoint(cap.center);
    //     }
    //     else
    //     {
    //         var rend = fp.GetComponentInChildren<Renderer>();
    //         if (rend != null)
    //         {
    //             radiusWorld = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.z);
    //             heightWorld = rend.bounds.size.y;
    //             centerWorld = rend.bounds.center;
    //         }
    //     }

    //     float moveDist = radiusWorld + spawnOffset;
    //     Vector3 spawn = centerWorld + dirFlat * moveDist;

    //     float chestY = fp.position.y + Mathf.Clamp(heightWorld * 0.25f, 0.2f, 1.2f) + spawnHeightOffset;
    //     spawn.y = chestY;

    //     RaycastHit hit;
    //     Vector3 rayOrigin = fp.position + Vector3.up * 0.2f;

    //     if (Physics.Raycast(rayOrigin, dirFlat, out hit, moveDist + 0.1f, obstacleMask))
    //     {
    //         spawn = hit.point + dirFlat * 0.12f;
    //         spawn.y = chestY;
    //     }

    //     return spawn;
    // }
}

// using UnityEngine;
// using System.Collections.Generic;
// public class WeaponManager : MonoBehaviour
// {
//     public List<Weapon> weapons = new List<Weapon>();
//     //Test
//     // void Start()
//     // {
//     //     var test = FindFirstObjectByType<Test>();
//     //     AddWeapon(test);
//     // }

//     [Header("Spawn positioning")]
//     public float spawnOffset = 0.12f;
//     public float spawnHeightOffset = 0.0f;
//     public LayerMask obstacleMask;
//     void Update()
//     {
//         var player = FindFirstObjectByType<PlayerAttack>();
//         Camera attackCamera = FindFirstObjectByType<Camera>();;
//         Vector3 camDir = attackCamera.transform.forward;
//         Vector3 posSpawn = ComputeSpawnPosition(player.transform, camDir);
//         float dt = Time.deltaTime;
//         foreach (var w in weapons)
//             w.Tick(dt, posSpawn);
//     }
//     Vector3 ComputeSpawnPosition(Transform fp, Vector3 dir)
//     {
//         Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);

//         if (dirFlat.sqrMagnitude < 0.0001f)
//             dirFlat = fp.forward;

//         dirFlat.Normalize();

//         CapsuleCollider cap = fp.GetComponent<CapsuleCollider>();

//         float radiusWorld = 0.5f;
//         float heightWorld = 1.0f;
//         Vector3 centerWorld = fp.position;

//         if (cap != null)
//         {
//             Vector3 lossy = fp.lossyScale;
//             float scaleXZ = Mathf.Max(lossy.x, lossy.z);
//             radiusWorld = cap.radius * scaleXZ;
//             heightWorld = cap.height * lossy.y;
//             centerWorld = fp.TransformPoint(cap.center);
//         }
//         else
//         {
//             var rend = fp.GetComponentInChildren<Renderer>();
//             if (rend != null)
//             {
//                 radiusWorld = Mathf.Max(rend.bounds.extents.x, rend.bounds.extents.z);
//                 heightWorld = rend.bounds.size.y;
//                 centerWorld = rend.bounds.center;
//             }
//         }

//         float moveDist = radiusWorld + spawnOffset;
//         UnityEngine.Vector3 spawn = centerWorld + dirFlat * moveDist;

//         float chestY = fp.position.y + Mathf.Clamp(heightWorld * 0.25f, 0.2f, 1.2f) + spawnHeightOffset;
//         spawn.y = chestY;

//         RaycastHit hit;
//         UnityEngine.Vector3 rayOrigin = fp.position + UnityEngine.Vector3.up * 0.2f;

//         if (Physics.Raycast(rayOrigin, dirFlat, out hit, moveDist + 0.1f, obstacleMask))
//         {
//             spawn = hit.point + dirFlat * 0.12f;
//             spawn.y = chestY;
//         }

//         return spawn;
//     }
//     public void AddWeapon(Weapon w)
//     {
//         weapons.Add(w);
//     }
// }