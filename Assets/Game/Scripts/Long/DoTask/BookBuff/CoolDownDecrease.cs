using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Cooldown Decrease")]
public class CoolDownDecrease : BookBuff
{
    [Tooltip("Percent, 0.05 = 5%")]
    public float amount = 0.05f;

    public override void Apply()
    {
        foreach (var weapon in WeaponManager.Instance.weapons)
        {
            weapon.ApplyCooldownReduction(amount);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:     amount = 0.05f; break;
            case Rarity.Uncommon:   amount = 0.08f; break;
            case Rarity.Rare:       amount = 0.12f; break;
            case Rarity.Epic:       amount = 0.18f; break;
            case Rarity.Legendary:  amount = 0.25f; break;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        return $"-{amount * 100f}% Weapon Cooldown";
    }
}
