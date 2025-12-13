using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Speed Increase")]
public class SpeedIncreaseBuff : BookBuff
{
    public float amount = 10f;
    private float currentSpeed = 0;
    public override void Apply(PlayerStatManager stats)
    {
        // stats.maxHP += amount;
        // stats.currentHP = Mathf.Min(stats.currentHP + amount, stats.maxHP);
        currentSpeed += amount;
        stats.ModifyMoveSpeed(currentSpeed);
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:     amount += 5f; break;
            case Rarity.Uncommon:   amount += 10f; break;
            case Rarity.Rare:       amount += 20f; break;
            case Rarity.Epic:       amount += 35f; break;
            case Rarity.Legendary:  amount += 50f; break;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        return $"+{amount} Max HP";
    }
}
