using UnityEngine;

/// ═══════════════════════════════════════════════════════════════════
///  UNLOCK DATABASE
///  ScriptableObject trung tâm chứa tất cả weapon và character.
///
///  Tạo asset: chuột phải → Create → ShopData → UnlockDatabase
///  Đặt trong Resources/ShopItem/UnlockDB (để load bằng code nếu cần)
///  hoặc kéo tay vào Inspector của ShopManager.
/// ═══════════════════════════════════════════════════════════════════
[CreateAssetMenu(menuName = "ShopData/UnlockDatabase")]
public class UnlockDatabase : ScriptableObject
{
    [Header("Weapons")]
    public UnlockItemDefinition[] weapons;

    [Header("Characters")]
    public UnlockItemDefinition[] characters;
}