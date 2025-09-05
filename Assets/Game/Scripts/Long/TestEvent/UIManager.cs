using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public Slider killProgressBar;
    public Slider eventTimerBar;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateKillProgress(int current, int max)
    {
        Debug.Log("Kill Progress: " + (float)current / max);
        killProgressBar.value = (float)current / max;
    }

    public void ShowEventTimer(float duration)
    {
        eventTimerBar.gameObject.SetActive(true);
        eventTimerBar.maxValue = duration;
        eventTimerBar.value = duration;
    }

    public void UpdateEventTimer(float timeLeft)
    {
        eventTimerBar.value = timeLeft;
    }

    public void HideKillProgress()
    {
        if (killProgressBar != null)
            killProgressBar.gameObject.SetActive(false);
    }

    public void ActiveKillProgress()
    {
        if (killProgressBar != null)
            killProgressBar.gameObject.SetActive(true);
    }

    public void HideEventTimer()
    {
        if (eventTimerBar != null)
            eventTimerBar.gameObject.SetActive(false);
    }


    [Header("Event UI")]
    public Slider captureBar;

    public void ShowCaptureBar(float maxValue) {
        if (captureBar != null) {
            captureBar.gameObject.SetActive(true);
            captureBar.maxValue = maxValue;
            captureBar.value = 0;
        }
    }

    public void UpdateCapturebar(float value) {
        if (captureBar != null) {
            captureBar.value = value;
        }
    }

    public void HideCaptureBar() {
        if (captureBar != null) {
            captureBar.gameObject.SetActive(false);
        }
    }
}
