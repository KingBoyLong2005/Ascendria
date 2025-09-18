using UnityEngine;
using Unity.Netcode;

public enum GamePhase {
    NormalWave,
    EventActive
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public NetworkVariable<GamePhase> currentPhase = new NetworkVariable<GamePhase>(GamePhase.NormalWave);
    public NetworkVariable<int> killCount = new NetworkVariable<int>(0);

    [Header("Progress")]
    public int killsToTriggerEvent = 5; // số quái cần giết để mở event

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddKill()
    {
        if (!IsServer) return; // chỉ server quản lý

        killCount.Value++;
        // gửi xuống client để update UI
        UpdateKillProgressClientRpc(killCount.Value, killsToTriggerEvent);

        if (currentPhase.Value == GamePhase.NormalWave)
        {
            EventManager.Instance.CheckEventTrigger();
        }
        else if (currentPhase.Value == GamePhase.EventActive)
        {
            EventManager.Instance.AddEventTime(0.3f);
        }
    }

    public void ResetProgress()
    {
        if (!IsServer) return;

        killCount.Value = 0;
        UpdateKillProgressClientRpc(0, killsToTriggerEvent);
    }

    [ClientRpc]
    private void UpdateKillProgressClientRpc(int current, int max)
    {
        // bảo đảm an toàn null-check
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateKillProgress(current, max);
    }
}
