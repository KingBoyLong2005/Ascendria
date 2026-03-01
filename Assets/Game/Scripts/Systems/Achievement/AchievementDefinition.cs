using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Achievement")]
public class AchievementDefinition : ScriptableObject
{
    public string id;
    public Sprite Icon;
    public string displayName;
    public string description;
    public int target;
}
