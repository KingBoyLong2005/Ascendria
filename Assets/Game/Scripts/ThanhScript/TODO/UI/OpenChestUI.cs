using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OpenChestUI : MonoBehaviour
{
    public Image itemIcon;
    public Text itemName;
    public Button takeButton;
    public Button discardButton;

    private Item currentItem; // reference to the rewarded item

    public void Show(Item item)
    {
        currentItem = item;
        itemIcon.sprite = item.Icon;
        itemName.text = item.name;

        gameObject.SetActive(true);

        takeButton.onClick.RemoveAllListeners();
        discardButton.onClick.RemoveAllListeners();

        takeButton.onClick.AddListener(() => TakeItem());
        discardButton.onClick.AddListener(() => DiscardItem());
    }

    private void TakeItem()
    {
        InventoryManager.Instance.AddItem(currentItem);
        ClosePopup();
    }

    private void DiscardItem()
    {
        Debug.Log("Item discarded: " + currentItem.name);
        ClosePopup();
    }

    private void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}

