using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [Header("UI Elements")]
    public Slider killProgressBar;
    public Slider eventTimerBar;
    
    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void UpdateKillProgress(int current, int max) {
        killProgressBar.value = (float)current / max;
    }

    public void ShowEventTimer(float duration) {
        eventTimerBar.gameObject.SetActive(true);
        eventTimerBar.maxValue = duration;
        eventTimerBar.value = duration;
    }

    public void UpdateEventTimer(float timeLeft) {
        eventTimerBar.value = timeLeft;
    }

    public void HideEventTimer() {
        eventTimerBar.gameObject.SetActive(false);
    }
}
