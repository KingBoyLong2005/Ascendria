using UnityEngine;

public abstract class UpgradableItem : ScriptableObject
{
    public Sprite Icon;
    public int level = 1;

    public abstract void LevelUp(Rarity rarity);

    // thêm hàm mô tả để UI hiển thị:
    public virtual string GetUpgradeDescription(Rarity rarity)
    {
        return "";
    }

}
