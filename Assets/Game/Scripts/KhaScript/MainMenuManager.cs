using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement; // Quan trọng để quản lý Scene

public class MainMenuManager : MonoBehaviour
{
    // Tên Scene chứa game chính của bạn (ví dụ: "GameScene")
    // public string gameSceneName = "MapNodeScene"; 
    public GameObject pannelSetting;
    public void PlayGame()
    {
        // Tải Scene chứa game chính
        //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.clickSfx);
        SceneManager.LoadScene("ReadyScene"); 
        Debug.Log("Starting Game...");
    }
    public void Shop()
    {
        //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.clickSfx);
        SceneManager.LoadScene("ShopScene");
    }
    public void Achive()
    {
        //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.clickSfx);
        SceneManager.LoadScene("AchiveScene");
    }
    public void Setting()
    {
        pannelSetting.SetActive(true);
    }

    public void QuitGame()
    {
        // Thoát ứng dụng (chỉ hoạt động trong build, không hoạt động trong Unity Editor)
        //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.clickSfx);
        Application.Quit();
        Debug.Log("Quitting Game...");

        // kiểm tra trong Editor:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}