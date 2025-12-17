using Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Health Bar displayed on Canvas
public class HealthBarUI : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI text;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetHealth(float currHealth, float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currHealth;
        text.text = $"{Mathf.RoundToInt(currHealth)}/{Mathf.RoundToInt(maxHealth)}";
    }
}
