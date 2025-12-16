using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Sneaker")]
public class ItemSneaker : Item
{
    public override void Apply(PlayerStatManager stats, int multiplier = 1)
    {
        stats.ModifyMoveSpeed(10);
    }

    public override void Remove(PlayerStatManager stats, int multiplier = 1)
    {
        throw new System.NotImplementedException();
    }
}