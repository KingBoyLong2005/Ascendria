using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ProfileCharacterLoader : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public ProfileCharacterData profile;

    [Header("Roots")]
    [SerializeField] private Transform visualRoot;
    // [Header("Weapon")]
    // [SerializeField] private Weapon wp;

    [Header("Runtime References")]
    public Animator Animator { get; private set; }
    public GameObject Model { get; private set; }
    

    [Header("Stats (runtime)")]
    public int MaxHP { get; private set; }
    public float MoveSpeed { get; private set; }
    // public float AttackSpeed { get; private set; }

    private void Awake()
    {
        // if (profile != null)
        //     ApplyProfile();
        MaxHP = profile.maxHP;
        MoveSpeed = profile.moveSpeed;
    }

    public void ApplyProfile()
    {
        // profile = data;

        ApplyStats();
        LoadModel();
        // LoadWeapon();
    }

    private void ApplyStats()
    {
        PlayerStatManager.Instance.baseMaxHealth = profile.maxHP;
        PlayerStatManager.Instance.baseMoveSpeed = profile.moveSpeed;
        // PlayerStatManager.Instance.base = profile.attackSpeed;
    }

    private void LoadModel()
    {
        if (profile.modelPrefab == null || visualRoot == null)
            return;

        if (Model != null)
            Destroy(Model);

        Model = Instantiate(profile.modelPrefab, visualRoot.position, visualRoot.rotation, visualRoot);
        // Model.transform.localScale = Vector3.one;  // Only scale reset needed; position/rotation handled by overload

        Animator = Model.GetComponentInChildren<Animator>();
        if (Animator != null && profile.animatorController != null)
            Animator.runtimeAnimatorController = profile.animatorController;
    }

    // private void LoadWeapon()
    // {
    //     if (profile.startingWeapon == null)
    //         return;
    //     FindFirstObjectByType<PlayerAttack>().wp = profile.startingWeapon;
    //     // foreach (Transform child in weaponRoot)
    //     //     Destroy(child.gameObject);

    //     // Instantiate(profile.startingWeapon.prefab, weaponRoot);
    // }
}
