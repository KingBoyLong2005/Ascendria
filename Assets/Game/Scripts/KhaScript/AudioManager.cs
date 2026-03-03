using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager (Persistent)");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Mixer & Groups")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string bgmGroupPath = "Master/BGM";
    [SerializeField] private string sfxGroupPath = "Master/SFX";

    [Header("SFX Throttling Settings")]
    [SerializeField] private int maxSimultaneousSFX = 10;
    [SerializeField] private float throttleInterval = 0.1f;
    private Dictionary<string, int> sfxPlayCount = new Dictionary<string, int>();

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    public event EventHandler<AudioEventArgs> OnSoundStarted;
    public event EventHandler<AudioEventArgs> OnMusicChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // TỰ ĐỘNG LOAD MIXER TỪ THƯ MỤC RESOURCES
        if (mainMixer == null)
        {
            mainMixer = Resources.Load<AudioMixer>("Audio/MainMixer"); // Tên phải khớp với tên file Mixer
        }

        sfxSource = GetComponent<AudioSource>();

        AudioSource[] sources = GetComponents<AudioSource>();
        bgmSource = (sources.Length > 1) ? sources[1] : gameObject.AddComponent<AudioSource>();

        // Tự động kết nối vào Audio Mixer Groups
        if (mainMixer != null)
        {
            bgmSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups(bgmGroupPath)[0];
            sfxSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups(sfxGroupPath)[0];
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        SetVolume("BGMVol", PlayerPrefs.GetFloat("BGM_Volume", 0.75f));
        SetVolume("SFXVol", PlayerPrefs.GetFloat("SFX_Volume", 0.75f));
    }

    // --- SFX VỚI CƠ CHẾ GIỚI HẠN (Dùng cho va chạm quái) ---
    public void PlaySFXLimited(AudioClip clip)
    {
        if (clip == null) return;

        string clipName = clip.name;
        if (!sfxPlayCount.ContainsKey(clipName)) sfxPlayCount[clipName] = 0;

        if (sfxPlayCount[clipName] < maxSimultaneousSFX)
        {
            // Thêm chút ngẫu nhiên về Pitch để âm thanh tự nhiên hơn (Mentor's Tip)
            sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

            sfxSource.PlayOneShot(clip);
            sfxPlayCount[clipName]++;

            // Reset đếm sau một khoảng thời gian ngắn
            Invoke(nameof(ResetSfxCount), throttleInterval);

            OnSoundStarted?.Invoke(this, new AudioEventArgs(clipName, clip.length));
        }
    }

    private void ResetSfxCount() => sfxPlayCount.Clear();

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.pitch = 1.0f; // Reset pitch về mặc định cho các âm thanh UI
        sfxSource.PlayOneShot(clip);
        OnSoundStarted?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || (bgmSource.clip == clip && bgmSource.isPlaying)) return;
        bgmSource.clip = clip;
        bgmSource.Play();
        OnMusicChanged?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    // --- HÀM CHO UI SETTINGS ---
    public void SetVolume(string parameterName, float sliderValue)
    {
        // Chuyển 0->1 sang -80dB -> 0dB
        float dB = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
        mainMixer.SetFloat(parameterName, dB);
    }

    public class AudioEventArgs : EventArgs
    {
        public string Name { get; }
        public float Duration { get; }
        public AudioEventArgs(string name, float duration) { Name = name; Duration = duration; }
    }
}