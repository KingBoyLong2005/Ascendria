using UnityEngine;

public class SettingPanel : MonoBehaviour
{
    public GameObject pannelSetting;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void turnOffSettingPanel()
    {
        pannelSetting.SetActive(false);
    }
}
