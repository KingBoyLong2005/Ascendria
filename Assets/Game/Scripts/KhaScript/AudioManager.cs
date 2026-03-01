using System;
using UnityEngine;
using UnityEngine.Audio; // Cần thiết nếu bạn dùng Audio Mixer sau này

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    // Cơ chế Lazy Initialization: Tự tìm hoặc tự tạo khi có người gọi AudioManager.Instance
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tìm trong toàn bộ các Scene xem có cái nào chưa
                _instance = FindFirstObjectByType<AudioManager>();

                if (_instance == null)
                {
                    // Nếu chưa có, tạo mới hoàn toàn một GameObject để chứa nó
                    GameObject go = new GameObject("AudioManager (Persistent)");
                    _instance = go.AddComponent<AudioManager>();
                    Debug.Log("<color=cyan>[AudioManager]</color> Tự động khởi tạo cho Menu Scene.");
                }
            }
            return _instance;
        }
    }

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    public event EventHandler<AudioEventArgs> OnSoundStarted;
    public event EventHandler<AudioEventArgs> OnMusicChanged;

    private void Awake()
    {
        // Kiểm tra Singleton để tránh tình trạng nhân bản khi load lại Scene
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // CHÌA KHÓA: Giữ Object này không bị hủy khi chuyển Scene
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // Cấu hình sfxSource từ Component có sẵn (RequireComponent)
        sfxSource = GetComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Cấu hình bgmSource bằng cách thêm mới một Component khác
        // Nếu đã có từ trước (do load lại scene), hãy tìm nó thay vì add thêm
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 1)
        {
            bgmSource = sources[1];
        }
        else
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
        OnSoundStarted?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Tránh việc phát lại từ đầu nếu bài nhạc đang phát chính là bài này
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
        OnMusicChanged?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    public class AudioEventArgs : EventArgs
    {
        public string Name { get; }
        public float Duration { get; }
        public AudioEventArgs(string name, float duration)
        {
            Name = name;
            Duration = duration;
        }
    }
}