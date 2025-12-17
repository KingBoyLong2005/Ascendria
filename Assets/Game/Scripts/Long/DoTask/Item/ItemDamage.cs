using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Damage")]
public class ItemDamage : Item
{
    public override void Apply(PlayerStatManager stats, int multiplier = 1)
    {
        stats.ModifyAttack(10);
    }

    public override void Remove(PlayerStatManager stats, int multiplier = 1)
    {
        throw new System.NotImplementedException();
    }
}