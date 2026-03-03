// ProfileCharacterLoader.cs
// Đọc stats từ ProfileCharacterData.RuntimeData thay vì đọc thẳng các field.
// PlayerStatManager không cần sửa gì.

using UnityEngine;

public class ProfileCharacterLoader : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public ProfileCharacterData profile;

    [Header("Roots")]
    [SerializeField] private Transform visualRoot;

    [Header("Runtime References")]
    public Animator    Animator { get; private set; }
    public GameObject  Model    { get; private set; }

    // ── Stats (PlayerStatManager đọc từ đây) ──────────────────
    public float MaxHP     => _data.maxHP;
    public float attack    => _data.attack;
    public float armor     => _data.armor;
    public float moveSpeed => _data.moveSpeed;
    public float luck      => _data.luck;
    public float wealth    => _data.wealth;
    public float wise      => _data.wise;

    private ProfileCharacterData.RuntimeData _data;

    private void Awake()
    {
        // Ưu tiên lấy profile từ PrefabDatabase (set bởi ReadyScene)
        if (PrefabDatabase.Instance != null && PrefabDatabase.Instance.selectedCharacter != null)
            profile = PrefabDatabase.Instance.selectedCharacter;

        if (profile == null)
        {
            Debug.LogError("[ProfileCharacterLoader] Không tìm thấy profile!");
            _data = new ProfileCharacterData.RuntimeData(); // fallback tránh null ref
            return;
        }

        // Lấy RuntimeData (base stats, hoặc base + upgrades sau khi có Save/Shop)
        _data = profile.GetRuntimeData();
    }

    // ── Model Loading ─────────────────────────────────────────

    public void ApplyProfile() => LoadModel();

    private void LoadModel()
    {
        if (profile == null || profile.modelPrefab == null || visualRoot == null)
            return;

        if (Model != null)
            Destroy(Model);

        Model = Instantiate(profile.modelPrefab, visualRoot.position, visualRoot.rotation, visualRoot);

        Animator = Model.GetComponentInChildren<Animator>();
        if (Animator != null && profile.animatorController != null)
            Animator.runtimeAnimatorController = profile.animatorController;
    }
}