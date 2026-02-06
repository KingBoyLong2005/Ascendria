
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/BookBuff")]
public abstract class BookBuff : UpgradableItem
{
    [TextArea] 
    public string description;   // mô tả chung
    public string buffId;
    public float value;          // giá trị tăng
    public string statTarget;    // tên stat muốn tăng (MoveSpeed / Crit / HP...)

    public override void LevelUp(Rarity rarity)
    {
        level++;

        switch (rarity)
        {
            case Rarity.Common: value *= 1.1f; break;
            case Rarity.Uncommon: value *= 1.2f; break;
            case Rarity.Rare: value *= 1.35f; break;
            case Rarity.Epic: value *= 1.5f; break;
            case Rarity.Legendary: value *= 2f; break;
        }
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        return $"Increase {statTarget} by {value}";
    }
    // buff áp thẳng vào PlayerStats
    public abstract void Apply();
}
