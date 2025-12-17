using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Heart")]
public class ItemHeart : Item
{
    public override void Apply(PlayerStatManager stats, int multiplier = 1)
    {
        stats.ModifyHealth(10);
    }

    public override void Remove(PlayerStatManager stats, int multiplier = 1)
    {
        throw new System.NotImplementedException();
    }
}