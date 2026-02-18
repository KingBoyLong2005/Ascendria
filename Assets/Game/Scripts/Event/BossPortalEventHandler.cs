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
        Debug.Log($"Mở Chest: {gate.name}");
    }
}
