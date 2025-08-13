using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{   
    [SerializeField] private Button CreateLobby;
    [SerializeField] public TMP_InputField CodeJoin;
    [SerializeField] private Button JoinLobby;
    private void Awake()
    {
        if (CreateLobby == null || JoinLobby == null || CodeJoin == null)
        {
            Debug.LogError("Một hoặc nhiều thành phần UI (CreateLobby, JoinLobby, CodeJoin) chưa được gán trong Inspector.");
            return;
        }

    }

    private void OnEnable()
    {
        CreateLobby.onClick.AddListener(OnCreateLobbyClicked);
        JoinLobby.onClick.AddListener(OnJoinLobbyClicked);
    }

    private void OnDisable()
    {
        CreateLobby.onClick.RemoveListener(OnCreateLobbyClicked);
        JoinLobby.onClick.RemoveListener(OnJoinLobbyClicked);
    }

    private  void OnCreateLobbyClicked()
    {
        CreateLobby.interactable = false; // Vô hiệu hóa nút trong khi xử lý
        Lobby.Instance.CreateLobby();

        SceneManager.LoadScene(1);

        // Debug.LogError("Tạo lobby thất bại, không chuyển scene.");
        CreateLobby.interactable = true; // Kích hoạt lại nút nếu thất bại
        
    }

    private void OnJoinLobbyClicked()
    {
        JoinLobby.interactable = false; // Vô hiệu hóa nút trong khi xử lý
        Lobby.Instance.joinLobbyByCode(CodeJoin.text);

        SceneManager.LoadScene(1);

        // Debug.LogError("Tham gia lobby thất bại, không chuyển scene.");
        JoinLobby.interactable = true; // Kích hoạt lại nút nếu thất bại
        
    }
}