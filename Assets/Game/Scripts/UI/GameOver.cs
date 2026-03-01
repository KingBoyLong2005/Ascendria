using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public GameObject PanelGameOver;
    public Button YesButton;
    public Button NoButton;

    public void Awake()
    {
        YesButton.onClick.AddListener(()=>PlayAgain());
        NoButton.onClick.AddListener(()=>BackToMenu());
    }
    public void GameOverActive()
    {
        PanelGameOver.SetActive(true);
        FindFirstObjectByType<TPCameraController>().isUIOpen = true;
    }
    public void PlayAgain()
    {
        Debug.Log("Play Again");
        SceneManager.LoadScene("ReadyScene");
    }
    public void BackToMenu()
    {
        Debug.Log("Back To Menu");
        SceneManager.LoadScene("MenuScene");
    }
    private void GameOverOverride()
    {
        float coinGet = InventoryManager.Instance.GetTotalCoins();
        float killGet = EnemyManager.Instance.GetKillCount();
        float levelGet = LevelManager.Instance.level;
        int bosskilCount = BossManager.Instance.bossKillCount;
        int levelHighestWeapon = GetHighestWeaponLevel();
        float moveSpeed = PlayerStatManager.Instance.MoveSpeed;
        float level = LevelManager.Instance.level;

        AchievementSystem.Instance.Initialize();


        //Start Endgame call
        var changes = new Dictionary<string, int>
        {
            // { "Item4Collect1000Gold", coinGet },
            // { "Item5ReachLevel20", 1 },
            // { "Item6Kill50Bosses", 1 },
            // { "Item7ReachSomeMoveSpeed", 1 },
            // { "Item8Reach100Hp", 1 },
            // { "Item9ReachLevel15Weapon", 1 },
            // { "Item10Kill1000Monsters", 1 },
            // { "Item11Open100Chest", 1 }
        };

        AchievementSystem.Instance.ApplyProgressChanges(changes);
        //End Endgame call

        //Call log progress
        AchievementSystem.Instance.LogSavedProgress();
    }
    public int GetHighestWeaponLevel()
    {
        int max = 0;
        foreach (var w in InventoryManager.Instance.ownedWeapons)
        {
            if (w != null && w.level > max)
                max = w.level;
        }
        return max;
    }
}
