using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource; // Dùng để phát tiếng động
    private AudioSource bgmSource; // Dùng để phát nhạc nền (Loop)

    // ĐỊNH NGHĨA CÁC SỰ KIỆN (Tín hiệu phát đi)
    public event EventHandler<AudioEventArgs> OnSoundStarted;
    public event EventHandler<AudioEventArgs> OnMusicChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else
        {
            Instance = this;
            SetupAudioSources();
        }
    }

    private void SetupAudioSources()
    {
        // Source 1: Phát SFX
        sfxSource = GetComponent<AudioSource>();
        sfxSource.loop = false;

        // Source 2: Phát BGM (Tạo thêm một AudioSource nữa bằng code)
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
    }

    // --- PHÁT HIỆU ỨNG ÂM THANH (SFX) ---
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip); // Lệnh phát nhạc thực tế

        // Phát tín hiệu EventHandler
        OnSoundStarted?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    // --- PHÁT NHẠC NỀN (BGM) ---
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play(); // Lệnh phát nhạc thực tế

        // Phát tín hiệu EventHandler
        OnMusicChanged?.Invoke(this, new AudioEventArgs(clip.name, clip.length));
    }

    // LỚP DỮ LIỆU SỰ KIỆN
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