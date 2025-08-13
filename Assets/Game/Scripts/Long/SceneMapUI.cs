using UnityEngine;
using UnityEngine.UI;

public class SceneMapUI : MonoBehaviour
{
    [SerializeField] private Button ButtonBack;

    private void Awake()
    {
        ButtonBack.onClick.AddListener(() =>
        {
            LoadScene.Instance.LoadSceneMap("NetworkScene");
        });
    }
}
