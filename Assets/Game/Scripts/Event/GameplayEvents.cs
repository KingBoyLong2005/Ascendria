using System;
using UnityEngine;

public static class GameplayEvents
{
    public static event Action<Interactable> OnChestInteracted;
    public static event Action<Interactable> OnBossGateInteracted;
    public static event Action<Interactable> OnEventInteracted;
    public static event Action<Item> OnLootChestCollected;
    public static int chestOpenCount = 0;
    public static void RaiseChestInteracted(Interactable chest)
    {
        OnChestInteracted?.Invoke(chest);
    }

    public static void RaiseBossGateInteracted(Interactable gate)
    {
        OnBossGateInteracted?.Invoke(gate);
    }

    public static void RaiseEventInteracted(Interactable eventObject)
    {
        OnEventInteracted?.Invoke(eventObject);
    }

    //Thêm item vào inventory ??
    public static void RaiseLootChestCollected(Item item)
    {
        OnLootChestCollected?.Invoke(item);
        chestOpenCount++;
    }
    public static void RessetChestOpenCount()
    {
        chestOpenCount = 0;
    }
}
