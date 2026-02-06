using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Luck Increase")]
public class LuckBuff : BookBuff
{
    public float amount = 10f;
    public override void Apply()
    {
        // stats.maxHP += amount;
        // stats.currentHP = Mathf.Min(stats.currentHP + amount, stats.maxHP);
        // PlayerStatManager.Instance.ModifyMoveSpeed(currentSpeed);
        PlayerStatManager.Instance.ModifyLuck(amount);
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:     amount = 7f; break;
            case Rarity.Uncommon:   amount = 8f; break;
            case Rarity.Rare:       amount = 10f; break;
            case Rarity.Epic:       amount = 11f; break;
            case Rarity.Legendary:  amount = 14f; break;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        return $"+{amount} Max Luck";
    }
}
