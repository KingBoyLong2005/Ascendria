using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Game Config")]
    public int maxChestNums = 70;
    public int maxInteractObjectNum = 7;
    public int maxEnemy = 100;
    

    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogError("GameConfig not found in Resources!");
                }
            }
            return _instance;
        }
    }
}
