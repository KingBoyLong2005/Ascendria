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
        Debug.Log("Mở OpenEventzxcv");
        switch (eventObject.name)
        {
            case "EventMiniBoss(Clone)":
                Debug.Log("Mở EventMiniBoss");
                EnemyManager.Instance.SpawnMiniBoss();
                InteractionUI.Instance.ShowMessage("Something showed up!");
                eventObject.Interacted();
                break;

            case "EventCatchOrb(Clone)":
                Debug.Log("Mở EventCatchOrb");
                eventObject.Interacted();
                // bỏ
                break;

            case "EventCaptureObject(Clone)":
                Debug.Log("Mở EventCaptureObject");
                var capture = eventObject.GetComponent<EventCaptureZone>();
                capture?.ActivateEvent();
                break;

            case "EventSacrificialAltar(Clone)":
                Debug.Log("Mở EventSacrificialAltar");
                EventSacrificialAltar(eventObject);
                break;

            case "EventTravelingMerchant(Clone)":
                Debug.Log("Mở EventTravelingMerchant");
                eventObject.Interacted();
                //bỏ
                break;

            case "EventGreedAltar(Clone)":
                Debug.Log("Mở EventGreedAltar");
                EnemyManager.Instance.IncreaseDifficultyMultiplier();
                PlayerStatManager.Instance.ModifyLuck(0, 1.2f);
                InteractionUI.Instance.ShowMessage("Increase difficulty and get 20% more luck!");
                eventObject.Interacted();
                break;
        }
    }

    private void EventSacrificialAltar(Interactable eventObject)
    {
        int usedTime = eventObject.UsedTime;
        float currentHealth = PlayerStatManager.Instance.CurrentHealth;
        float maxHealth = PlayerStatManager.Instance.MaxHealth;

        float[] hpCostsPercent = { 0.25f, 0.5f, 0.75f, 0.99f };
        int[] rewards = { 30, 50, 70, 100 };

        if (usedTime < 0 || usedTime >= hpCostsPercent.Length)
            return;

        float requiredHp = maxHealth * hpCostsPercent[usedTime];

        // Không đủ máu
        if (currentHealth < requiredHp)
        {
            InteractionUI.Instance.ShowMessage("Not enough HP!");
            return;
        }

        // Đủ máu → trừ máu
        PlayerStatManager.Instance.TakeDamage(requiredHp);

        // Thưởng coin
        InventoryManager.Instance.AddEventCoin(rewards[usedTime]);

        // Tăng số lần dùng (tự hủy nếu = 4)
        eventObject.IncreaseUsedTime();

        // TODO: UI success
        InteractionUI.Instance.ShowMessage("Sacrifice for greed!");
    }    
}
