using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    public GameObject root;   // chính Panel Tooltip
    public TMP_Text titleText;
    public TMP_Text typeText;
    public TMP_Text descText;
    public TMP_Text statsText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (root != null)
        {
            root.SetActive(false);
        }
        Hide();
    }

    public void Show(ItemData data, Vector3 _)
    {
        if (!data || root == null) return;
        titleText.text = data.itemName;
        typeText.text = data.itemType.ToString();
        descText.text = data.description;
        statsText.text = FormatStats(data);
        root.SetActive(true);
        // bám theo chuột
        Vector3 pos = Input.mousePosition;
        transform.position = pos;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    string FormatStats(ItemData d)
    {
        string b = d.buffValue != 0 ? $"+{d.buffValue}" : "";
        string db = d.debuffValue != 0 ? $"-{d.debuffValue}" : "";
        if (b == "" && db == "") return "";
        if (b != "" && db != "") return $"{b} / {db}";
        return b != "" ? b : db;
    }

    void Update()
    {
        if (root != null && root.activeSelf)
            transform.position = Input.mousePosition + new Vector3(16f, -16f, 0f);
    }
}
