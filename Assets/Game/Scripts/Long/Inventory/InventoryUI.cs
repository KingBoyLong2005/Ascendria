using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI refs")]
    public GameObject panel;        // InventoryPanel
    public Transform gridRoot;      // ScrollView/Viewport/Content
    public GameObject itemSlotPrefab;
    public KeyCode toggleKey = KeyCode.I;

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnChanged += Rebuild;
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnChanged -= Rebuild;
    }

    void Start()
    {
        if (panel) panel.SetActive(false);
        Rebuild();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && panel)
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) Rebuild();
        }
    }

    public void Rebuild()
    {
        if (gridRoot == null || InventoryManager.Instance == null) return;

        // Clear cũ
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        // Build mới
        foreach (var item in InventoryManager.Instance.items)
        {
            var go = Instantiate(itemSlotPrefab, gridRoot);
            var slot = go.GetComponent<ItemSlotUI>();
            slot.Bind(item);
        }
    }
}
