using UnityEngine;
using Unity.Netcode;

public class EventManager : NetworkBehaviour {
    public static EventManager Instance;

    public enum EventType {
        None,
        CaptureTheFlag,
        CatchTheOrb,
        EscortObject,
        CollectDrops,
        SlotMachine
    }

    [Header("Event Prefabs")]
    public GameObject flagPrefab;
    public GameObject orbPrefab;
    public GameObject escortPrefab;
    public GameObject escortDestinationPrefab;

    [Header("Event Settings")]
    public float eventDuration = 30f;
    private float timer;
    private bool eventOngoing = false;
    private EventType currentEvent = EventType.None;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void CheckEventTrigger() {
        if (!IsServer) return;

        if (GameManager.Instance.killCount.Value >= GameManager.Instance.killsToTriggerEvent
            && GameManager.Instance.currentPhase.Value == GamePhase.NormalWave)
        {
            StartEvent();
        }
    }

    private void StartEvent() {
        if (!IsServer) return;

        GameManager.Instance.currentPhase.Value = GamePhase.EventActive;
        HideKillProgressClientRpc(); // -> cho tất cả client

        currentEvent = EventType.CaptureTheFlag;
        // currentEvent = EventType.CaptureTheFlag;
        // currentEvent = EventType.CaptureTheFlag;
        // currentEvent = EventType.CaptureTheFlag;
        Debug.Log("Event xuất hiện: " + currentEvent);

        eventOngoing = true;
        timer = eventDuration;
        ShowEventTimerClientRpc(eventDuration); // -> cho tất cả client

        if (currentEvent == EventType.CaptureTheFlag) {
            SpawnFlag();
        } else if (currentEvent == EventType.CatchTheOrb) {
            SpawnOrb();
        } else if (currentEvent == EventType.EscortObject) {
            SpawnEscort();
        } else {
            EnemySpawner.Instance.SpawnEventEnemies();
        }

        InvokeRepeating(nameof(UpdateEventTimer), 1f, 1f);
    }

    private void UpdateEventTimer() {
        if (!IsServer || !eventOngoing) return;

        timer -= 1f;
        UpdateEventTimerClientRpc(timer); // -> cho tất cả client

        if (timer <= 0) {
            EndEvent(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryCompleteEventServerRpc(ServerRpcParams rpcParams = default) {
        if (eventOngoing) {
            EndEvent(true);
        }
    }

    public void EndEvent(bool success) {
        if (!eventOngoing) return;

        Debug.Log(success
            ? $"Event {currentEvent} HOÀN THÀNH - Nhận thưởng!"
            : $"Event {currentEvent} THẤT BẠI - Không có thưởng.");

        eventOngoing = false;
        CancelInvoke(nameof(UpdateEventTimer));

        HideEventTimerClientRpc();
        ActiveKillProgressClientRpc();
        GameManager.Instance.ResetProgress();

        if (currentEvent == EventType.CaptureTheFlag) {
            var flag = FindFirstObjectByType<Flag>();
            if (flag != null) {
                // flag sẽ handle UI hide bằng RPC trong OnDestroy của nó
                flag.gameObject.GetComponent<NetworkObject>().Despawn(true);
                Destroy(flag.gameObject);
            }
        }
        if (currentEvent == EventType.CatchTheOrb) {
            var orb = FindFirstObjectByType<Orb>();
            if (orb != null) Destroy(orb.gameObject);
        }
        if (currentEvent == EventType.EscortObject) {
            EndEscortEvent(false);
        }
        var escortDestination = FindFirstObjectByType<ParticleSystem>();
        if (escortDestination != null) Destroy(escortDestination.gameObject);

        GameManager.Instance.currentPhase.Value = GamePhase.NormalWave;
        EnemySpawner.Instance.SpawnNormalWave();
        currentEvent = EventType.None;
    }

    public void AddEventTime(float extraTime) {
        if (!IsServer || !eventOngoing) return;

        timer += extraTime;
        if (timer > eventDuration) timer = eventDuration;
        UpdateEventTimerClientRpc(timer);
    }

    void SpawnFlag() {
        Vector3 pos = EnemySpawner.Instance.GetRandomSpawnPoint();
        var flag = Instantiate(flagPrefab, pos, Quaternion.identity);
        flag.GetComponent<NetworkObject>().Spawn(true);
    }

    void SpawnOrb() {
        Vector3 pos = EnemySpawner.Instance.GetRandomSpawnPoint();
        var orb = Instantiate(orbPrefab, pos, Quaternion.identity);
        orb.GetComponent<NetworkObject>().Spawn(true);
    }

    GameObject escortInstance;
    GameObject escortDestination;

    void SpawnEscort() {
        var spawns = EnemySpawner.Instance.spawnPoints;

        int startIndex = Random.Range(0, spawns.Length);
        int endIndex = Random.Range(0, spawns.Length);
        while (endIndex == startIndex) {
            endIndex = Random.Range(0, spawns.Length);
        }

        Vector3 startPos = spawns[startIndex].position;
        Vector3 endPos   = spawns[endIndex].position;

        escortInstance = Instantiate(escortPrefab, startPos, Quaternion.identity);
        escortInstance.GetComponent<NetworkObject>().Spawn(true);

        escortDestination = Instantiate(escortDestinationPrefab, endPos, Quaternion.identity);
        escortDestination.GetComponent<NetworkObject>().Spawn(true);

        escortInstance.GetComponent<EscortObject>().pathPoints = new Transform[] {
            escortDestination.transform
        };
    }

    public void EndEscortEvent(bool success) {
        if (escortInstance != null) {
            escortInstance.GetComponent<NetworkObject>().Despawn(true);
            Destroy(escortInstance);
        }
        if (escortDestination != null) {
            escortDestination.GetComponent<NetworkObject>().Despawn(true);
            Destroy(escortDestination);
        }

        EndEvent(success);
    }

    // ----- ClientRpc UI wrappers -----
    [ClientRpc]
    private void ShowEventTimerClientRpc(float duration)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowEventTimer(duration);
    }

    [ClientRpc]
    private void UpdateEventTimerClientRpc(float timeLeft)
    {
        if (UIManager.Instance != null) UIManager.Instance.UpdateEventTimer(timeLeft);
    }

    [ClientRpc]
    private void HideEventTimerClientRpc()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideEventTimer();
    }

    [ClientRpc]
    private void HideKillProgressClientRpc()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideKillProgress();
    }

    [ClientRpc]
    private void ActiveKillProgressClientRpc()
    {
        if (UIManager.Instance != null) UIManager.Instance.ActiveKillProgress();
    }

    [ClientRpc]
    private void UpdateKillProgressClientRpc(int current, int max)
    {
        if (UIManager.Instance != null) UIManager.Instance.UpdateKillProgress(current, max);
    }
}
