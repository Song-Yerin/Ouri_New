using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;

[System.Serializable]
public class SceneMusicPair
{
    public string sceneName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource cutsceneSource;

    [Header("Scene & Cutscene Music Table")]
    [Header("Scene BGM Table")]
    public SceneMusicPair[] bgmClips = new SceneMusicPair[]
    {
        new SceneMusicPair { sceneName = "Title", clip = null },
        new SceneMusicPair { sceneName = "FIrstCutScene", clip = null },
        new SceneMusicPair { sceneName = "YJ_Forest_Night", clip = null },
        new SceneMusicPair { sceneName = "PipeScene 1", clip = null },
        new SceneMusicPair { sceneName = "Practice", clip = null },
        new SceneMusicPair { sceneName = "YJ _Forest_Day", clip = null },
        new SceneMusicPair { sceneName = "waterway", clip = null },
        new SceneMusicPair { sceneName = "City_Day 2", clip = null },
        new SceneMusicPair { sceneName = "CityGliding2", clip = null },
        new SceneMusicPair { sceneName = "City_Day 4", clip = null },
        new SceneMusicPair { sceneName = "SpaceShipLauncher", clip = null },
    };

    public SceneMusicPair[] cutsceneClips;


    [Header("Transition Settings")]
    public bool smoothTransition = true;
    public float fadeTime = 1.0f;

    private string currentSceneName;
    private bool isCutscenePlaying = false;
    private int lastSamplePosition = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        isCutscenePlaying = false;  // ÄÆ¾À Áß »óÅÂ ÃÊ±âÈ­

        Debug.Log($"[SoundManager] Scene Loaded: {currentSceneName}");
        PlaySceneBGM(currentSceneName); // ¹«Á¶°Ç ¾À ÁøÀÔ ½Ã BGM Àç»ý

        RegisterAllCutscenesInScene();
    }


    private void RegisterAllCutscenesInScene()
    {
        var directors = FindObjectsOfType<PlayableDirector>(true);
        foreach (var dir in directors)
        {
            dir.played -= OnCutsceneStart;
            dir.stopped -= OnCutsceneEnd;
            dir.played += OnCutsceneStart;
            dir.stopped += OnCutsceneEnd;

            if (dir.state == PlayState.Playing)
            {
                Debug.Log($"[SoundManager] ÀÌ¹Ì Àç»ý ÁßÀÎ ÄÆ¾À Áï½Ã °¨Áö: {dir.playableAsset?.name}");
                OnCutsceneStart(dir);
            }
        }

        Debug.Log($"[SoundManager] ÄÆ¾À ÀÚµ¿ µî·Ï ¿Ï·á ({directors.Length}°³ ¹ß°ßµÊ)");
    }

    // ===============================
    // ¾À ¹è°æ BGM
    // ===============================
    public void PlaySceneBGM(string sceneName)
    {
        bgmSource.Stop();

        SceneMusicPair pair = FindClip(sceneName, bgmClips);
        if (pair == null) return;

        bgmSource.clip = pair.clip;
        bgmSource.loop = true;

        if (smoothTransition)
            StartCoroutine(FadeAndPlay(bgmSource, fadeTime));
        else
        {
            bgmSource.volume = 1f;
            bgmSource.Play();
        }
    }

    // ===============================
    // ÄÆ¾À ÀÚµ¿ °¨Áö¿ë BGM
    // ===============================
    private void OnCutsceneStart(PlayableDirector dir)
    {
        string cutsceneName = dir.playableAsset != null ? dir.playableAsset.name : dir.name;
        Debug.Log($"[SoundManager] °¨ÁöµÈ ÄÆ¾À ÀÌ¸§: {cutsceneName}");
        SceneMusicPair pair = FindClip(cutsceneName, cutsceneClips);
        if (pair == null)
        {
            Debug.Log($"[SoundManager] ÄÆ¾À '{cutsceneName}'¿¡ ´ëÀÀµÇ´Â À½¾Ç ¾øÀ½");
            return;
        }

        StartCoroutine(SwitchToCutscene(pair.clip));
    }

    private void OnCutsceneEnd(PlayableDirector dir)
    {
        ResumeSceneBGM();
    }

    private IEnumerator SwitchToCutscene(AudioClip clip)
    {
        isCutscenePlaying = true;
        lastSamplePosition = bgmSource.timeSamples;

        if (smoothTransition)
            yield return StartCoroutine(FadeOut(bgmSource, fadeTime));
        else
            bgmSource.Stop();

        cutsceneSource.clip = clip;
        cutsceneSource.volume = 1f;
        cutsceneSource.Play();
    }

    // ===============================
    // ÄÆ¾À ¼öµ¿ Àç»ý¿ë (¿À¹ö·Îµù 2°¡Áö)
    // ===============================

    // (1) ÄÆ¾À ÀÌ¸§ + ÀÚµ¿ º¹±Í ½Ã°£ ÁöÁ¤ ¹öÀü
    public void PlayCutsceneBGM(string cutsceneName, float duration)
    {
        SceneMusicPair pair = FindClip(cutsceneName, cutsceneClips);
        if (pair == null)
        {
            Debug.LogWarning($"[SoundManager] '{cutsceneName}'¿¡ ¸Â´Â ÄÆ¾À À½¾ÇÀ» Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            return;
        }

        StartCoroutine(SwitchToCutscene(pair.clip));
        StartCoroutine(AutoResumeAfterDelay(duration));
    }

    // (2) ÄÆ¾À ÀÌ¸§¸¸ (¼öµ¿ º¹±Í)
    public void PlayCutsceneBGM(string cutsceneName)
    {
        SceneMusicPair pair = FindClip(cutsceneName, cutsceneClips);
        if (pair == null)
        {
            Debug.LogWarning($"[SoundManager] '{cutsceneName}'¿¡ ¸Â´Â ÄÆ¾À À½¾ÇÀ» Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            return;
        }

        StartCoroutine(SwitchToCutscene(pair.clip));
    }

    private IEnumerator AutoResumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResumeSceneBGM();
    }

    // ===============================
    // ¾À º¹±Í BGM
    // ===============================
    public void ResumeSceneBGM()
    {
        isCutscenePlaying = false;

        if (cutsceneSource.isPlaying)
        {
            if (smoothTransition)
                StartCoroutine(FadeOut(cutsceneSource, fadeTime));
            else
                cutsceneSource.Stop();
        }

        if (bgmSource.clip != null)
        {
            bgmSource.timeSamples = Mathf.Clamp(lastSamplePosition, 0, bgmSource.clip.samples - 1);
            bgmSource.Play();

            if (smoothTransition)
                StartCoroutine(FadeIn(bgmSource, fadeTime));
        }
    }

    // ===============================
    // À¯Æ¿
    // ===============================
    private SceneMusicPair FindClip(string sceneName, SceneMusicPair[] list)
    {
        foreach (var item in list)
        {
            if (item.sceneName == sceneName)
                return item;
        }
        return null;
    }

    private IEnumerator FadeAndPlay(AudioSource source, float time)
    {
        yield return StartCoroutine(FadeOut(source, time));
        source.Play();
        yield return StartCoroutine(FadeIn(source, time));
    }

    private IEnumerator FadeIn(AudioSource src, float time)
    {
        src.volume = 0;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            src.volume = Mathf.Lerp(0, 1, t / time);
            yield return null;
        }
        src.volume = 1;
    }

    private IEnumerator FadeOut(AudioSource src, float time)
    {
        float startVol = src.volume;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            src.volume = Mathf.Lerp(startVol, 0, t / time);
            yield return null;
        }
        src.Stop();
        src.volume = startVol;
    }
}
