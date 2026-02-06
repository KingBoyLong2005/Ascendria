using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Health Increase")]
public class HealthIncreaseBuff : BookBuff
{
    public float amount = 10f;
    public override void Apply()
    {
        // stats.maxHP += amount;
        // stats.currentHP = Mathf.Min(stats.currentHP + amount, stats.maxHP);
        PlayerStatManager.Instance.ModifyHealth(amount);
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:     amount += 5f; break;
            case Rarity.Uncommon:   amount += 8f; break;
            case Rarity.Rare:       amount += 10f; break;
            case Rarity.Epic:       amount += 20f; break;
            case Rarity.Legendary:  amount += 25f; break;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        return $"+{amount} Max HP";
    }
}
