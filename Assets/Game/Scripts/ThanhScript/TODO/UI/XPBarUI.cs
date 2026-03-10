using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    private Slider slider;
    private Image fillImage;
    
    [Header("Rainbow Effect")]
    public float rainbowSpeed = 2f;
    private bool isRainbowActive = false;
    private Color originalColor;
    private float rainbowTimer = 0f;
    
    // Lưu giá trị XP thực tế để khôi phục sau
    private float savedCurrentXP;
    private float savedMaxXP;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        fillImage = slider.fillRect.GetComponent<Image>();
        // Lưu màu gốc
        if (fillImage != null)
            originalColor = fillImage.color;
    }

    private void Update()
    {
        if (isRainbowActive)
        {
            rainbowTimer += Time.unscaledDeltaTime * rainbowSpeed;
            
            // Tạo màu cầu vồng bằng HSV
            float hue = Mathf.Repeat(rainbowTimer, 1f);
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
            
            if (fillImage != null)
                fillImage.color = rainbowColor;
                
            // Giữ thanh XP luôn full khi đang có hiệu ứng
            slider.value = slider.maxValue;
        }
    }
    public void SetXP(float currXP, float xpToNext)
    {
        // Lưu giá trị thực tế
        savedCurrentXP = currXP;
        savedMaxXP = xpToNext;
        
        // Nếu đang có hiệu ứng rainbow thì không cập nhật UI
        if (isRainbowActive)
        {
            Debug.Log($"<color=orange>[XPBarUI]</color> SetXP (saved but not applied) → {currXP}/{xpToNext}");
            return;
        }
            
        slider.maxValue = xpToNext;
        slider.value = currXP;
    }
    public void StartRainbowEffect()
    {
        isRainbowActive = true;
        rainbowTimer = 0f;

        // Set thanh XP về full
        slider.value = slider.maxValue;

    }
    public void StopRainbowEffect()
    {
        isRainbowActive = false;

        if (fillImage != null)
            fillImage.color = originalColor;
        
        // Khôi phục giá trị XP thực tế
        slider.maxValue = savedMaxXP;
        slider.value = savedCurrentXP;

    }
}