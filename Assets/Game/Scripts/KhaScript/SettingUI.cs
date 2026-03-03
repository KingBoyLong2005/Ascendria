using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // Key để lưu vào bộ nhớ máy
    private const string BGM_KEY = "BGM_Volume";
    private const string SFX_KEY = "SFX_Volume";
    [SerializeField] private Toggle muteToggle;

    private void Start()
    {
        // 1. Load giá trị đã lưu, nếu chưa có thì mặc định là 0.75f (cho đỡ chói tai)
        float savedBGM = PlayerPrefs.GetFloat(BGM_KEY, 0.75f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

        // 2. Cập nhật giao diện Slider
        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        // 3. Áp dụng ngay lập tức vào AudioMixer thông qua AudioManager
        // Lưu ý: Tên hàm trong AudioManager của bạn là SetVolume (theo bản cập nhật trước)
        AudioManager.Instance.SetVolume("BGMVol", savedBGM);
        AudioManager.Instance.SetVolume("SFXVol", savedSFX);

        // 4. Đăng ký sự kiện thay đổi
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // Load trạng thái Mute (0 là không mute, 1 là mute)
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        muteToggle.isOn = isMuted;
        ApplyMute(isMuted);

        muteToggle.onValueChanged.AddListener(val => {
            ApplyMute(val);
            PlayerPrefs.SetInt("IsMuted", val ? 1 : 0);
        });
    }

    private void ApplyMute(bool isMuted)
    {
        // Cách chuyên nghiệp: AudioListener.pause sẽ ngắt toàn bộ âm thanh đầu ra
        AudioListener.pause = isMuted;

        // Nếu bạn muốn nhạc vẫn chạy ngầm nhưng không có tiếng:
        // AudioListener.volume = isMuted ? 0 : 1;
    }

    private void OnBGMChanged(float value)
    {
        AudioManager.Instance.SetVolume("BGMVol", value);
        PlayerPrefs.SetFloat(BGM_KEY, value); // Lưu lại ngay khi kéo
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance.SetVolume("SFXVol", value);
        PlayerPrefs.SetFloat(SFX_KEY, value); // Lưu lại ngay khi kéo
    }

    private void OnDisable()
    {
        // Đảm bảo dữ liệu được ghi xuống đĩa cứng khi đóng bảng Setting
        PlayerPrefs.Save();
    }
}