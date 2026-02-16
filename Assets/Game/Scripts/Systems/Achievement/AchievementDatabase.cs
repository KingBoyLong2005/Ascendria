using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/AchievementDB")]
public class AchievementDatabase : ScriptableObject
{
    public AchievementDefinition[] achievements;
}
