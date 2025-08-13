using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text LobbyCode;
    [SerializeField] private Button ButtonMap1;
    [SerializeField] private Button ButtonMap2;
    public static LobbyUI Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        ButtonMap1.onClick.AddListener(() =>
        {
            LoadScene.Instance.LoadSceneMap("SceneMap1");
        });

        ButtonMap2.onClick.AddListener(() =>
        {
            LoadScene.Instance.LoadSceneMap("SceneMap2");
        });
    }
    public void UpdateLobbyCodeJoin(string lobbyCode)
    {
        LobbyCode.text = string.IsNullOrEmpty(lobbyCode) ? "Không có mã lobby" : lobbyCode;
    }
}
