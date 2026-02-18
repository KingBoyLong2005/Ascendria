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
        //LLogic
        Debug.Log($"Mở Chest: {chest.name}");
    }
}
