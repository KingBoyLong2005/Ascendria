using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// ═══════════════════════════════════════════════════════════════════
///  UNLOCK ITEM DEFINITION  —  Wrapper
///  Bọc ngoài WeaponSO hoặc CharacterSO.
///  Chứa unlockCost và đọc name/icon từ SO thật bên trong.
///  Implement IUnlockable để ShopSlotUI và ShopManager dùng chung.
///
///  Tạo asset: chuột phải → Create → ShopData → UnlockItemDefinition
///
///  CÁCH DÙNG:
///  - Tạo 1 asset cho mỗi weapon/character
///  - Kéo WeaponSO hoặc CharacterSO vào field [sourceWeapon / sourceCharacter]
///  - Chỉ điền 1 trong 2, cái còn lại để trống
///  - Điền id (phải khớp với GameSaveSystem) và unlockCost
/// ═══════════════════════════════════════════════════════════════════
[CreateAssetMenu(menuName = "ShopData/UnlockItemDefinition")]
public class UnlockItemDefinition : ShopItemDefinition, IUnlockable
{
    [Header("Identity")]
    public int    unlockCost;

    [Header("Source — chỉ điền 1 trong 2")]
    public Weapon sourceWeapon;
    public ProfileCharacterData sourceCharacter;

    // ── IUnlockable ────────────────────────────────────────────────
    
    public string Id         => id;
    public int    UnlockCost => unlockCost;

    public string DisplayName
    {
        get
        {
            if (sourceWeapon    != null) return sourceWeapon.weaponName;
            if (sourceCharacter != null) return sourceCharacter.displayName;
            return id;
        }
    }

    public Sprite Icon
    {
        get
        {
            if (sourceWeapon    != null) return sourceWeapon.Icon;
            if (sourceCharacter != null) return sourceCharacter.iconChar;
            return null;
        }
    }
}

public interface IUnlockable
{
    string Id          { get; }   // dùng để check GameSaveSystem
    string DisplayName { get; }   // hiện trên slot
    Sprite Icon        { get; }   // icon trên slot
    int    UnlockCost  { get; }   // giá mua
}