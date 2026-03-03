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
        GameOverOverride();
    }
    public void BackToMenu()
    {
        Debug.Log("Back To Menu");
        SceneManager.LoadScene("MenuScene");
        GameOverOverride();
    }
    private void GameOverOverride()
    {
        int silverGet = (int)InventoryManager.Instance.GetTotalSilver();
        int killGet = (int)EnemyManager.Instance.GetKillCount();
        int levelGet = LevelManager.Instance.level;
        int bosskilCount = BossManager.Instance.bossKillCount;
        int levelHighestWeapon = GetHighestWeaponLevel();
        int moveSpeed = (int)PlayerStatManager.Instance.MoveSpeed;
        int healthGet = (int)PlayerStatManager.Instance.MaxHealth;
        int chestOpenCount = GameplayEvents.chestOpenCount;

        GameplayEvents.RessetChestOpenCount();
        AchievementSystem.Instance.Initialize();


        //Start Endgame call
        var changes = new Dictionary<string, int>
        {
            { "Item4Collect1000Gold", silverGet },
            { "Item5ReachLevel20", levelGet },
            { "Item6Kill50Bosses", bosskilCount },
            { "Item7ReachSomeMoveSpeed", moveSpeed },
            { "Item8Reach100Hp", healthGet },
            { "Item9ReachLevel15Weapon", levelHighestWeapon },
            { "Item10Kill1000Monsters", killGet },
            { "Item11Open100Chest", chestOpenCount }
        };

        AchievementSystem.Instance.ApplyProgressChanges(changes);
        //End Endgame call

        //Call log progress
        AchievementSystem.Instance.LogSavedProgress();

        GameSaveSystem.Instance.AddCoin(silverGet);
        // GameplayEvents.ResetChestOpenCount();
        // BossManager.Instance.ResetBossKillCount();
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
