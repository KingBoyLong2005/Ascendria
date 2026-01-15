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
    public float MaxHP { get; private set; }
    public float attack { get; private set; }
    public float armor { get; private set; }
    public float moveSpeed { get; private set; }
    public float luck { get; private set; }
    public float wealth { get; private set; }
    public float wise { get; private set; }
    // public float AttackSpeed { get; private set; }

    private void Awake()
    {
        MaxHP = profile.maxHP;
        attack = profile.CharacterAttack;
        armor = profile.Armor;
        moveSpeed = profile.MoveSpeed;
        luck = profile.Luck;
        wealth = profile.Wealth;
        wise = profile.Wise;

        // Dùng để test (nhớ bỏ)
        // ApplyProfile();
    }

    public void ApplyProfile()
    {
        // profile = data;
        // ApplyStats();
        LoadModel();
        // LoadWeapon();
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

}
