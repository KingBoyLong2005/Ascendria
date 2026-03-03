using System;
using UnityEngine;

public class BossGateEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        GameplayEvents.OnBossGateInteracted += OpenGate;
    }

    private void OnDisable()
    {
        GameplayEvents.OnBossGateInteracted -= OpenGate;
    }

    private void OpenGate(Interactable gate)
    {
        //LLogic
        BossManager.Instance.SpawnGateBoss(MapManager01.Instance.GetBossGatePosition());
        Debug.Log($"Triệu hồi boss: {gate.name}");
    }
}
