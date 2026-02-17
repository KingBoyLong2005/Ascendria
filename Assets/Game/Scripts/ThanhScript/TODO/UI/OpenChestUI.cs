using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenChestUI : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public TMP_Text discardAttempt;
    public Button takeButton;
    public Button discardButton;
    public GameObject panel;

    private Item currentItem; // reference to the rewarded item

    public void Show(Item item)
    {
        GameManager.Instance.PauseGame();
        FindFirstObjectByType<TPCameraController>().isUIOpen = true;

        currentItem = item;
        itemIcon.sprite = item.Icon;
        itemName.text = item.name;
        //itemDescription = item.des;
        discardAttempt.text = "Remain: " + PlayerStatManager.Instance.Discard.ToString();

        panel.SetActive(true);

        takeButton.onClick.RemoveAllListeners();
        discardButton.onClick.RemoveAllListeners();

        takeButton.onClick.AddListener(() => TakeItem());
        discardButton.onClick.AddListener(() => DiscardItem());
    }

    private void TakeItem()
    {
        InventoryManager.Instance.AddItem(currentItem);
        ClosePopup();
        FindFirstObjectByType<TPCameraController>().isUIOpen = false;
        GameManager.Instance.ResumeGame();
    }

    private void DiscardItem()
    {
        if (PlayerStatManager.Instance.Discard > 0)
        {
            PlayerStatManager.Instance.ModifyDiscard(-1f);
            ClosePopup();
            FindFirstObjectByType<TPCameraController>().isUIOpen = false;
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Debug.Log("No discard attempt left");
        }
    }

    private void ClosePopup()
    {
        panel.SetActive(false);
    }
}

