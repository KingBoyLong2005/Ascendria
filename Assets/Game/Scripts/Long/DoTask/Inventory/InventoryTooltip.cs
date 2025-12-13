using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    public GameObject root;   // panel
    // public TMP_Text titleText;
    // public TMP_Text typeText;
    // public TMP_Text descText;
    // public TMP_Text statsText;
    // public Image iconImage;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (root != null) root.SetActive(false);
    }

    /// <summary>
    /// Show tooltip for any InventoryItemBase (WeaponData or ItemData are subclasses)
    /// screenPos is where the tooltip should be anchored (usually pointer position)
    /// </summary>
    public void Show(InventoryItemBase data, Vector3 screenPos)
    {
        if (data == null || root == null) return;

        // titleText.text = data.displayName;
        // descText.text = data.description ?? "";

        // // icon
        // if (iconImage != null) iconImage.sprite = data.icon;

        // // typeText + statsText depend on actual type
        // if (data is WeaponData w)
        // {
        //     typeText.text = $"Weapon • {w.weaponType}";
        //     statsText.text = $"DMG: {w.baseDamage}\nRate: {w.attackRate:F2}/s\nRange: {w.range:F1}";
        // }
        // else if (data is ItemData it)
        // {
        //     typeText.text = $"Item • {it.itemType}";
        //     string s = "";
        //     if (it.buffValue != 0f) s += $"+{it.buffValue}\n";
        //     if (it.debuffValue != 0f) s += $"-{it.debuffValue}\n";
        //     statsText.text = s.Trim();
        // }
        // else
        // {
        //     typeText.text = "Item";
        //     statsText.text = "";
        // }

        root.SetActive(true);
        // set tooltip near mouse by default; caller can pass desired pos
        transform.position = screenPos;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (root != null && root.activeSelf)
            transform.position = Input.mousePosition + new Vector3(16f, -16f, 0f);
    }
}
