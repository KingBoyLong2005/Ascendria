using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public Button button;

    WeaponData boundData;
    System.Action<WeaponData> onSelected;

    // call to set up display
    public void Setup(WeaponData data, System.Action<WeaponData> callback)
    {
        boundData = data;
        onSelected = callback;

        if (icon != null)
            icon.sprite = data != null ? data.icon : null;
        if (nameText != null)
            nameText.text = data != null ? data.weaponName : "None";
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        onSelected?.Invoke(boundData);
    }
}
