using System;
using UnityEngine;

public class ChestEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        GameplayEvents.OnChestInteracted += OpenChest;
    }

    private void OnDisable()
    {
        GameplayEvents.OnChestInteracted -= OpenChest;
    }

    private void OpenChest(Interactable chest)
    {
        var obj = ItemManager.Instance.GetRandomItem();
        UIManager.Instance.GameplayEvents_OnLootChestCollected(obj);

        chest.Interacted();

        Debug.Log($"Mở Chest: {chest.name}");
    }
}
