using System;
using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
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
                //ChestLogic function
                Debug.Log($"Mở Chestzxcvzxcvzxczxcv: {e.Target.name}");
                break;

            case InteractionType.Event:
                //ChestLogic function
                Debug.Log($"Trigger Event: {e.Target.name}");
                break;

            case InteractionType.BossGate:
                //ChestLogic function
                Debug.Log($"Trigger Boss Gate Event: {e.Target.name}");
                break;
        }
    }    
}
