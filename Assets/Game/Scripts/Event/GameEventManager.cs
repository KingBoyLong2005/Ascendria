using UnityEngine;
using System;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null){
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Khởi tạo các Handler ở đây
        gameObject.AddComponent<ChestEventHandler>();
        gameObject.AddComponent<EventObjectHandler>();
        gameObject.AddComponent<BossGateEventHandler>();
    }

    private void OnEnable()
    {
        GameEventSystem.OnInteract += HandleInteraction;
    }

    private void OnDisable()
    {
        GameEventSystem.OnInteract -= HandleInteraction;
    }

    private void HandleInteraction(object sender, InteractionEventArgs e)
    {
        switch (e.InteractionType)
        {
            case InteractionType.Chest:
                //ChestLogic call
                GameplayEvents.RaiseChestInteracted(e.Target);
                Debug.Log($"Mở Chestzxcvzxcvzxczxcv: {e.Target.name}");
                break;

            case InteractionType.Event:
                //EventLogic call
                GameplayEvents.RaiseEventInteracted(e.Target);
                break;

            case InteractionType.BossGate:
                //ChestLogic call
                GameplayEvents.RaiseBossGateInteracted(e.Target);
                Debug.Log($"Trigger Boss Gate Event: {e.Target.name}");
                break;
        }
    }
}