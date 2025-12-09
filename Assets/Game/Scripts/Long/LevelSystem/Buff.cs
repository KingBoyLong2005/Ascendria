using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Buff")]
public class Buff : Weapon
{
    public BuffType buffType;
    public float buffValue;

    public override void Attack(WeaponContext ctx)
    {
        // Apply buff to owner if needed; for passive buffs, perhaps apply elsewhere (e.g., in player stats manager)
        // For now, empty as buff might be applied on addition to inventory
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                buffValue = 5f;
                break;
            case Rarity.Uncommon:
                buffValue = 10f;
                break;
            case Rarity.Rare:
                buffValue = 20f;
                break;
            case Rarity.Epic:
                buffValue = 40f;
                break;
            case Rarity.Legendary:
                buffValue = 80f;
                break;
        }

        level++;
    }
}