using UnityEngine;
using UnityEngine.SceneManagement;

public class BackMenu : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BackMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
