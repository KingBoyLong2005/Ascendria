using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<ItemData> items = new List<ItemData>();
    public event Action OnChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(ItemData newItem)
    {
        if (newItem == null) return;
        items.Add(newItem);
        OnChanged?.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;
        if (items.Remove(item)) OnChanged?.Invoke();
    }
}
