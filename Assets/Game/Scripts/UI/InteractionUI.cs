using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [Header("Button")]
    [SerializeField] private GameObject eButton;

    [Header("Message")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup messageCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    private Coroutine fadeRoutine;

    public bool IsShowing => eButton != null && eButton.activeInHierarchy;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show() => eButton.SetActive(true);
    public void Hide() => eButton.SetActive(false);

    public void ShowMessage(string message)
    {
        if (messageText == null || messageCanvasGroup == null)
            return;

        // stop fade cũ
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        // set text mới
        messageText.text = message;

        // hiện rõ ngay
        messageCanvasGroup.gameObject.SetActive(true);
        messageCanvasGroup.alpha = 1f;

        // chạy fade mới từ đầu
        fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            messageCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        messageCanvasGroup.alpha = 0f;
        messageCanvasGroup.gameObject.SetActive(false);
        fadeRoutine = null;
    }
}