using Mono.CSharp;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Button BackMenu;
    public string SceneName = "";

    private TPCameraController mouseActive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseActive = FindFirstObjectByType<TPCameraController>();
        BackMenu.onClick.AddListener(BackToMenu);
    }

    void Update()
    {   
        
        if(Input.GetKey(KeyCode.LeftAlt))
        {
            mouseActive.isUIOpen = true;
        }
        else
        {
            mouseActive.isUIOpen = false;
        }

        if(Input.GetKey(KeyCode.Escape))
        {
            CheckEndGame("Done");
        }
    }
    private void BackToMenu()
    {
        Debug.Log("End Game Back to Menu");
        if(SceneName != "")
        {
            SceneManager.LoadScene(SceneName);
        }
        else
        {
            Debug.LogWarning("Scene chưa có");
        }
    }

    private bool CheckEndGame(string State)
    {
        if(State == "End")
        {
            Debug.Log("End Game Back to Menu");
            if(SceneName != "")
            {
                SceneManager.LoadScene(SceneName);
            }
            else
            {
                Debug.LogWarning("Scene chưa có");
            }
            return true;
        }
        else if(State == "Not Done")
        {
            Debug.Log("Chưa kết thúc");
            return true;
        }
        else
        {
            Debug.LogWarning("Sai trạng thái");
            return false;
        }
    }
}
