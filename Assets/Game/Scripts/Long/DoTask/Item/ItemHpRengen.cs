using UnityEngine;

[CreateAssetMenu(fileName = "ItemHPRegen", menuName = "Item/HPRegen")]
public class ItemHPRegen : Item
{
    [Header("Stats")]
    [Tooltip("Hồi máu mỗi giây")]
    public float hpRegenPerSecond = 5f;

    public override void Apply(int multiplier = 1)
    {
        if(!PlayerStatManager.Instance.activeHpRegen)
        {
            PlayerStatManager.Instance.activeHpRegen = true;
        }
        float totalRegen = hpRegenPerSecond * multiplier;
        PlayerStatManager.Instance.ValueHpRegenPerSecond += totalRegen;
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalRegen} HP/s Regen (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        float totalRegen = hpRegenPerSecond * multiplier;
        // stats.hpRegenPerSecond -= totalRegen;
        // stats.hpRegenPerSecond = Mathf.Max(0f, stats.hpRegenPerSecond);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalRegen} HP/s Regen (x{multiplier})");
    }
}