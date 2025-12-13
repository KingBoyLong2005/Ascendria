using UnityEngine;
using System.Collections.Generic;

public class BookBuffManager : MonoBehaviour
{
    public static BookBuffManager Instance { get; private set; }

    private PlayerStatManager stats;

    // Danh sách buff đang active
    private List<BookBuff> activeBuffs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        // Chờ InventoryManager READY rồi mới apply buff
        if (InventoryManager.Instance != null)
        {
            HandleInventoryReady(null, System.EventArgs.Empty);
        }
        else
        {
            InventoryManager.Instance.OnInventoryReady += HandleInventoryReady;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryReady -= HandleInventoryReady;
    }

    private void HandleInventoryReady(object sender, System.EventArgs e)
    {
        stats = PlayerStatManager.Instance;

        // Apply lại toàn bộ buff đang active trong inventory
        ReapplyAll();
    }

    // Apply một buff mới (dùng khi level-up hoặc nhặt item)
    public void ApplyBuff(BookBuff buff)
    {
        if (buff == null || stats == null) return;

        activeBuffs.Add(buff);
        buff.Apply(stats);
    }

    // public void RemoveBuff(BookBuff buff)
    // {
    //     if (buff == null) return;

    //     if (activeBuffs.Contains(buff))
    //     {
    //         buff.Remove(stats);
    //         activeBuffs.Remove(buff);
    //     }
    // }

    // Apply lại tất cả buff player đã có
    public void ReapplyAll()
    {
        activeBuffs.Clear();

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        foreach (var buff in inv.activeBookBuffs)
        {
            activeBuffs.Add(buff);
            buff.Apply(stats);
        }
    }
}
