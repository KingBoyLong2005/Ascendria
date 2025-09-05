using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public enum EventType
    {
        None,
        CaptureTheFlag,
        CatchTheOrb,
        EscortObject,
        CollectDrops,
        SlotMachine
    }

    [Header("Event PreFab")]
    public GameObject flagPrefab;
    public GameObject orbPrefab;
    public GameObject escortPrefab;
    public GameObject escortDestinationPrefab;

    [Header("Event Settings")]
    public float eventDuration = 15f; // thời gian event
    private float timer;
    private bool eventOngoing = false;
    private EventType currentEvent = EventType.None;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CheckEventTrigger()
    {
        if (GameManager.Instance.killCount >= GameManager.Instance.killsToTriggerEvent
            && GameManager.Instance.currentPhase == GamePhase.NormalWave)
        {
            StartEvent();
        }
    }


    void StartEvent()
    {
        // Reset progress
        GameManager.Instance.currentPhase = GamePhase.EventActive;
        // KHÔNG ResetProgress ở đây
        UIManager.Instance.HideKillProgress();

        // Random event
        // currentEvent = (EventType)Random.Range(1, 6); // từ 1 -> 5
        // currentEvent = EventType.CaptureTheFlag;
        // currentEvent = EventType.CatchTheOrb;
        // currentEvent = EventType.EscortObject;

        currentEvent = (EventType)Random.Range(1, 4); // từ 1 -> 5

        Debug.Log("Event xuất hiện: " + currentEvent);

        // Bật trạng thái event
        eventOngoing = true;
        timer = eventDuration;
        UIManager.Instance.ShowEventTimer(eventDuration);

        // Spawn quái/đồ vật nếu cần (sau này thêm)
        // EnemySpawner.Instance.SpawnEventEnemies();

        if (currentEvent == EventType.CaptureTheFlag)
        {
            Debug.Log("Start event " + currentEvent);
            // SpawnFlag();
            SpawnFlag();
        }
        else if (currentEvent == EventType.CatchTheOrb)
        {
            Debug.Log("Start event " + currentEvent);
            // SpawnFlag();
            SpawnOrb();
        }
        else if (currentEvent == EventType.EscortObject)
        {
            Debug.Log("Start event " + currentEvent);
            SpawnEscort();
        }
        else
        {
            EnemySpawner.Instance.SpawnEventEnemies();
        }
    }

    void Update()
    {
        if (eventOngoing)
        {
            // Đếm ngược thời gian
            timer -= Time.deltaTime;
            UIManager.Instance.UpdateEventTimer(timer);

            // Nhấn F để hoàn thành event
            if (Input.GetKeyDown(KeyCode.F))
            {
                EndEvent(true);
            }

            // Hết giờ thì fail
            if (timer <= 0)
            {
                EndEvent(false);
            }
        }
    }

    public void EndEvent(bool success)
    {
        if (!eventOngoing) return;

        Debug.Log(success
            ? $"Event {currentEvent} HOÀN THÀNH - Nhận thưởng!"
            : $"Event {currentEvent} THẤT BẠI - Không có thưởng.");

        // Reset state
        eventOngoing = false;
        currentEvent = EventType.None;
        UIManager.Instance.HideEventTimer();

        // Bật kill progress khi hết event
        UIManager.Instance.ActiveKillProgress();
        GameManager.Instance.ResetProgress();

        // Xóa flag nếu còn tồn tại
        if (currentEvent == EventType.CaptureTheFlag)
        {
            var flag = FindFirstObjectByType<Flag>();
            if (flag != null) Destroy(flag.gameObject);
        }

        // Xóa orb nếu còn tồn tại
        if (currentEvent == EventType.CatchTheOrb)
        {
            var orb = FindFirstObjectByType<Orb>();
            if (orb != null) Destroy(orb.gameObject);
        }

        // Xóa escort nếu còn tồn tại
        if (currentEvent == EventType.EscortObject)
        {
            EndEscortEvent(false);
        }

        var escortDestination = FindFirstObjectByType<ParticleSystem>();
        if (escortDestination != null) Destroy(escortDestination.gameObject);

        // Code cũ
        GameManager.Instance.currentPhase = GamePhase.NormalWave;
        EnemySpawner.Instance.SpawnNormalWave();
        // Code cũ

    }

    // Tăng thời gian khi giết thêm quái
    public void AddEventTime(float extraTime)
    {
        if (eventOngoing)
        {
            timer += extraTime;
            if (timer > eventDuration) timer = eventDuration; // không vượt quá max
            UIManager.Instance.UpdateEventTimer(timer);
            Debug.Log($"+{extraTime}s cho event! Thời gian còn lại: {timer:F1}");
        }
    }

    void SpawnFlag()
    {
        // Tận dụng list spawn point của quái
        Vector3 pos = EnemySpawner.Instance.GetRandomSpawnPoint();
        Instantiate(flagPrefab, pos, Quaternion.identity);
    }
    void SpawnOrb()
    {
        Vector3 pos = EnemySpawner.Instance.GetRandomSpawnPoint();
        Instantiate(orbPrefab, pos, Quaternion.identity);
    }

    // Escort
    GameObject escortInstance;
    GameObject escortDestination;

    void SpawnEscort() {
        var spawns = EnemySpawner.Instance.spawnPoints;

        int startIndex = Random.Range(0, spawns.Length);
        int endIndex = Random.Range(0, spawns.Length);

        // ép buộc khác nhau
        while (endIndex == startIndex) {
            endIndex = Random.Range(0, spawns.Length);
        }

        Vector3 startPos = spawns[startIndex].position;
        Vector3 endPos   = spawns[endIndex].position;
        // Spawn escort
        escortInstance = Instantiate(escortPrefab, startPos, Quaternion.identity);

        // Spawn đích (cột sáng)
        escortDestination = Instantiate(escortDestinationPrefab, endPos, Quaternion.identity);

        // Gán đường đi (có thể là A* hoặc navmesh, ở đây demo 2 điểm start→end)
        escortInstance.GetComponent<EscortObject>().pathPoints = new Transform[] {
            escortDestination.transform
        };
    }

    public void EndEscortEvent(bool success) {
        EndEvent(success);

        if (escortInstance != null) Destroy(escortInstance);
        if (escortDestination != null) Destroy(escortDestination);
    }

}
