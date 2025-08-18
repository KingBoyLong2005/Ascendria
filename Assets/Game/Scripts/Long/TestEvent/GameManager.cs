using UnityEngine;

public enum GamePhase {
    NormalWave,
    EventActive
}

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("Game State")]
    public GamePhase currentPhase = GamePhase.NormalWave;

    [Header("Progress")]
    public int killCount = 0;
    public int killsToTriggerEvent = 5; // số quái cần giết để mở event

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void AddKill() {
        killCount++;
        UIManager.Instance.UpdateKillProgress(killCount, killsToTriggerEvent);

        // Kiểm tra trigger event
        EventManager.Instance.CheckEventTrigger();
    }

    public void ResetProgress() {
        killCount = 0;
        UIManager.Instance.UpdateKillProgress(0, killsToTriggerEvent);
    }
}
