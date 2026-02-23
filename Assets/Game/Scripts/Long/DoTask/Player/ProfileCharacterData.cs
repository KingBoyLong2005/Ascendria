// ProfileCharacterData.cs
// ScriptableObject chứa base stats của nhân vật.
// Có thêm RuntimeData (class lồng bên trong) làm "cầu nối" cho Save System / Shop sau này.

using UnityEngine;

[CreateAssetMenu(menuName = "Character/Profile Character Data")]
public class ProfileCharacterData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;

    [Header("Base Stats")]
    public float maxHP;
    public float CharacterAttack;
    public float Armor;
    public float MoveSpeed;

    [Tooltip("Increase drop rate of Silver, increase chance of higher rarity options when level up")]
    public float Luck;
    [Tooltip("Increase the amount of Gold and Silver that drop")]
    public float Wealth;
    [Tooltip("Increase the amount of each XPGem")]
    public float Wise;

    [Header("Visual")]
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;

    [Header("Combat")]
    public Weapon startingWeapon;

    // ── Runtime Data ──────────────────────────────────────────
    // Lưu stats "thực tế" sẽ dùng khi vào game (base + upgrades từ Shop).
    // Hiện tại: copy thẳng từ base stats bên trên.
    // Sau này: SaveSystem / Shop chỉ cần gọi BuildRuntimeData() rồi modify fields.

    [System.Serializable]
    public class RuntimeData
    {
        public float maxHP;
        public float attack;
        public float armor;
        public float moveSpeed;
        public float luck;
        public float wealth;
        public float wise;
    }

    // Cache — tạo một lần, dùng lại trong cùng session
    private RuntimeData _runtimeData;

    /// <summary>
    /// Lấy RuntimeData của nhân vật này (tạo mới nếu chưa có).
    /// TODO (Save System): Thay phần build bên trong bằng: load save → cộng upgrade.
    /// </summary>
    public RuntimeData GetRuntimeData()
    {
        if (_runtimeData == null)
            _runtimeData = BuildRuntimeData();
        return _runtimeData;
    }

    /// <summary>
    /// Tạo RuntimeData từ base stats. Gọi lại khi cần reset (vd: sau khi mua upgrade).
    /// </summary>
    public RuntimeData BuildRuntimeData()
    {
        // TODO (Save System): cộng thêm upgrade stats ở đây
        _runtimeData = new RuntimeData
        {
            maxHP     = maxHP,
            attack    = CharacterAttack,
            armor     = Armor,
            moveSpeed = MoveSpeed,
            luck      = Luck,
            wealth    = Wealth,
            wise      = Wise,
        };
        return _runtimeData;
    }

    // Reset cache khi asset bị thay đổi trong Editor
    private void OnValidate() => _runtimeData = null;
}