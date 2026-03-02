using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [SerializeField] private GameObject pressEPanel;

    public bool IsShowing => pressEPanel != null && pressEPanel.activeInHierarchy;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show() => pressEPanel.SetActive(true);
    public void Hide() => pressEPanel.SetActive(false);
}