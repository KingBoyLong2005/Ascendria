using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetXP(float currXP, float xpToNext)
    {
        slider.maxValue = xpToNext;
        slider.value = currXP;
    }
}
