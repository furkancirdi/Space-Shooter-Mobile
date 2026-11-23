using UnityEngine;
using UnityEngine.SceneManagement;

// Ses efektleri ve muzik. Sahneler arasinda yasayan tek bir nesne uzerinde
// kucuk bir AudioSource havuzu kullanir; sahneye hicbir sey eklemek gerekmez.
public static class Sfx
{
    private const int PoolSize = 12;
    public static float MusicVolume = 0.3f;
    public static float SfxVolume = 1f;

    private static GameObject host;
    private static AudioSource[] pool;
    private static AudioSource music;
    private static AudioListener ownListener;
    private static int next;

    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        EnsureHost();

        AudioSource src = pool[next];
        next = (next + 1) % PoolSize;
        src.pitch = pitch;
        src.PlayOneShot(clip, volume * SfxVolume);
    }

    public static void PlayMusic()
    {
        EnsureHost();
        AudioClip clip = GameAssets.I != null ? GameAssets.I.music : null;
        if (clip == null) return;

        music.volume = MusicVolume;
        if (music.clip == clip && music.isPlaying) return;
        music.clip = clip;
        music.loop = true;
        music.Play();
    }

    // Oyun bitince muzigi kisar (0-1 arasi carpan)
    public static void DuckMusic(float factor)
    {
        if (music != null) music.volume = MusicVolume * factor;
    }

    private static void EnsureHost()
    {
        if (host != null) return;

        host = new GameObject("Audio");
        Object.DontDestroyOnLoad(host);

        pool = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            pool[i] = host.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }

        music = host.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.ignoreListenerPause = true;

        // Sahnede dinleyici yoksa (ana menu) kendi dinleyicimizi kullan
        ownListener = host.AddComponent<AudioListener>();
        SceneManager.sceneLoaded += (scene, mode) => RefreshListener();
        RefreshListener();
    }

    private static void RefreshListener()
    {
        if (ownListener == null) return;
        ownListener.enabled = false;
        AudioListener[] all = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        bool sceneHasListener = false;
        foreach (AudioListener l in all)
            if (l != ownListener && l.isActiveAndEnabled) sceneHasListener = true;
        ownListener.enabled = !sceneHasListener;
    }
}
