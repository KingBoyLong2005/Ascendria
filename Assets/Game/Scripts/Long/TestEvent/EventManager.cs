using UnityEngine;

public class EventManager : MonoBehaviour {

    public enum EventType
    {
    None,
    CaptureTheFlag,
    CatchTheOrb,
    EscortObject,
    CollectDrops,
    SlotMachine
    }

    public static EventManager Instance;

    [Header("Event Settings")]
    public float eventDuration = 15f; // thời gian event
    private float timer;
    private bool eventOngoing;
    private EventType currentEvent = EventType.None;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void CheckEventTrigger() {
        if (GameManager.Instance.killCount >= GameManager.Instance.killsToTriggerEvent 
            && GameManager.Instance.currentPhase == GamePhase.NormalWave) {
            StartEvent();
        }
    }

    void StartEvent() {
        // Reset progress
        GameManager.Instance.currentPhase = GamePhase.EventActive;
        GameManager.Instance.ResetProgress();

        // Random event
        currentEvent = (EventType)Random.Range(1, 6); // từ 1 -> 5
        Debug.Log("Event xuất hiện: " + currentEvent);

        // Bật trạng thái event
        eventOngoing = true;
        timer = eventDuration;
        UIManager.Instance.ShowEventTimer(eventDuration);

        // Spawn quái/đồ vật nếu cần (sau này thêm)
        EnemySpawner.Instance.SpawnEventEnemies();
    }

    void Update() {
        if (eventOngoing) {
            // Đếm ngược thời gian
            timer -= Time.deltaTime;
            UIManager.Instance.UpdateEventTimer(timer);

            // Nhấn F để hoàn thành event
            if (Input.GetKeyDown(KeyCode.F)) {
                EndEvent(true);
            }

            // Hết giờ thì fail
            if (timer <= 0) {
                EndEvent(false);
            }
        }
    }

    public void EndEvent(bool success) {
        if (!eventOngoing) return;

        Debug.Log(success 
            ? $"Event {currentEvent} HOÀN THÀNH - Nhận thưởng!" 
            : $"Event {currentEvent} THẤT BẠI - Không có thưởng.");

        // Reset state
        eventOngoing = false;
        currentEvent = EventType.None;
        UIManager.Instance.HideEventTimer();
        GameManager.Instance.currentPhase = GamePhase.NormalWave;

        // Spawn lại wave thường
        EnemySpawner.Instance.SpawnNormalWave();
    }
}
