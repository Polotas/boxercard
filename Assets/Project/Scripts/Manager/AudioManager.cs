using UnityEngine;

public static class AudioManager
{
    private static GameObject audioRoot;
    private static AudioSource bgSource;
    private static AudioSource fxSource;

    private static float bgVolume = 1f;
    private static float fxVolume = 1f;

    private static bool bgMuted = false;
    private static bool fxMuted = false;

    private static AudioClip buttonClip; // Som padrão de botão
    private static AudioClip coinCollect;
    
    public enum AudioType { BG, FX }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (audioRoot != null) return;

        audioRoot = new GameObject("AudioManager");
        Object.DontDestroyOnLoad(audioRoot);

        bgSource = audioRoot.AddComponent<AudioSource>();
        bgSource.loop = true;

        fxSource = audioRoot.AddComponent<AudioSource>();
        fxSource.loop = false;

        UpdateVolumes();
    }

    // === Play normal ===
    public static void Play(AudioClip clip, AudioType type)
    {
        if (clip == null) return;
        Init();

        if (type == AudioType.BG)
        {
            bgSource.clip = clip;
            bgSource.Play();
        }
        else if (type == AudioType.FX)
        {
            fxSource.PlayOneShot(clip, fxVolume * (fxMuted ? 0f : 1f));
        }
    }

    public static void Stop(AudioType type)
    {
        Init();
        if (type == AudioType.BG && bgSource.isPlaying)
            bgSource.Stop();

        if (type == AudioType.FX && fxSource.isPlaying)
            fxSource.Stop();
    }

    // === Play Button ===
    public static void SetButtonClip(AudioClip clip) => buttonClip = clip;

    public static void SetCoinCollect(AudioClip clip) => coinCollect = clip;
    
    public static void PlayButtonSound()
    {
        if (buttonClip != null)
        {
            Play(buttonClip, AudioType.FX);
        }
        else
        {
            Debug.LogWarning("Button sound not set. Use AudioManager.SetButtonClip().");
        }
    }
    
    public static void PlayCollectCoin()
    {
        if (coinCollect != null)
        {
            Play(coinCollect, AudioType.FX);
        }
        else
        {
            Debug.LogWarning("Button sound not set. Use AudioManager.SetButtonClip().");
        }
    }

    // === Volume ===
    public static void SetVolume(AudioType type, float volume)
    {
        Init();
        volume = Mathf.Clamp01(volume);

        if (type == AudioType.BG)
            bgVolume = volume;
        else
            fxVolume = volume;

        UpdateVolumes();
    }

    public static float GetVolume(AudioType type)
    {
        return type == AudioType.BG ? bgVolume : fxVolume;
    }

    // === Mute ===
    public static void SetMute(AudioType type, bool mute)
    {
        Init();
        if (type == AudioType.BG)
            bgMuted = mute;
        else
            fxMuted = mute;

        UpdateVolumes();
    }

    public static bool IsMuted(AudioType type)
    {
        return type == AudioType.BG ? bgMuted : fxMuted;
    }

    // === Helpers ===
    private static void UpdateVolumes()
    {
        if (bgSource != null)
            bgSource.volume = bgMuted ? 0f : bgVolume;

        if (fxSource != null)
            fxSource.volume = fxMuted ? 0f : fxVolume;
    }
}
