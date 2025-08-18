using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public ItemData data;

    public void Bind(ItemData d)
    {
        data = d;
        if (icon) icon.sprite = d.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null)
            InventoryTooltip.Instance.Show(data, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance?.Hide();
    }
}
