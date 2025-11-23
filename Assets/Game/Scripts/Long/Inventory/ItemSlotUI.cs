using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image icon;              // gán Image hiển thị icon trong prefab
    InventoryItemBase itemData;

    /// <summary>Gán data cho slot (call khi spawn)</summary>
    public void Bind(InventoryItemBase data)
    {
        itemData = data;
        if (icon != null)
            icon.sprite = data != null ? data.icon : null;

        // nếu muốn ẩn icon khi null:
        if (icon != null)
            icon.enabled = data != null && data.icon != null;
    }

}
