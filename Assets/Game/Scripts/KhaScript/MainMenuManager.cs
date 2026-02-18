// using UnityEngine;

// public class MainMenuManager : MonoBehaviour
// {
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }


using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement; // Quan trọng để quản lý Scene

public class MainMenuManager : MonoBehaviour
{
    // Tên Scene chứa game chính của bạn (ví dụ: "GameScene")
    // public string gameSceneName = "MapNodeScene"; 

    /// <summary>
    /// Hàm được gọi khi nhấn nút Play.
    /// </summary>
    public void PlayGame()
    {
        // Tải Scene chứa game chính
        SceneManager.LoadScene("ReadyScene"); 
        Debug.Log("Starting Game...");
    }
    public void Shop()
    {
        SceneManager.LoadScene("ShopScene");
    }
    public void Achive()
    {
        SceneManager.LoadScene("AchiveScene");
    }
    /// <summary>
    /// Hàm được gọi khi nhấn nút Quit.
    /// </summary>
    public void QuitGame()
    {
        // Thoát ứng dụng (chỉ hoạt động trong build, không hoạt động trong Unity Editor)
        Application.Quit();
        Debug.Log("Quitting Game...");

        // Dòng code sau dùng để kiểm tra trong Editor:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}