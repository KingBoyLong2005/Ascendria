using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlScheme;

public class AchievementLoadTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Initialize
        AchievementSystem.Instance.Initialize();

        //Start Endgame call
        var changes = new Dictionary<string, int>
        {
            { "Item4Collect1000Gold", 1 },
            { "Item5ReachLevel20", 1 },
            { "Item6Kill50Bosses", 1 },
            { "Item7ReachSomeMoveSpeed", 1 },
            { "Item8Reach100Hp", 1 },
            { "Item9ReachLevel15Weapon", 1 },
            { "Item10Kill1000Monsters", 1 },
            { "Item11Open100Chest", 1 }
        };

        AchievementSystem.Instance.ApplyProgressChanges(changes);
        //End Endgame call

        //Call log progress
        AchievementSystem.Instance.LogSavedProgress();
    }
}
