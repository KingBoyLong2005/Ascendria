using JetBrains.Annotations;
using UnityEditor.SearchService;
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
}
