using System;
using UnityEngine;

public class EventObjectHandler : MonoBehaviour
{
    private void OnEnable()
    {
        GameplayEvents.OnEventInteracted += OpenEvent;
    }

    private void OnDisable()
    {
        GameplayEvents.OnEventInteracted -= OpenEvent;
    }

    private void OpenEvent(Interactable eventObject)
    {
        switch (eventObject.name)
        {
            case "EventMiniBoss":
                Debug.Log("Mở EventMiniBoss");

                break;

            case "EventCatchOrb":
                Debug.Log("Mở EventCatchOrb");

                // bỏ
                break;

            case "EventCaptureObject":
                Debug.Log("Mở EventCaptureObject");
                break;

            case "EventSacrificialAltar":
                Debug.Log("Mở EventSacrificialAltar");
                break;

            case "EventTravelingMerchant":
                Debug.Log("Mở EventTravelingMerchant");

                //bỏ
                break;

            case "EventGreedAltar":
                Debug.Log("Mở EventGreedAltar");
                break;
        }
    }
}
