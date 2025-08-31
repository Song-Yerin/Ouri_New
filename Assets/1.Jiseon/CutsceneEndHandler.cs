using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "NextScene"; // 이동할 씬 이름
    public CanvasGroup fadeCanvas; // UI용 페이드 패널 (검은색 이미지에 CanvasGroup 붙여둠)
    public float fadeDuration = 1f;

    [Header("YR2에서 활성화할 오브젝트들")]
    public GameObject[] objectsToActivate; // 인스펙터에 원하는 오브젝트 넣기

    void Start()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        if (director != null)
            director.stopped += OnCutsceneEnd;

        // 시작 시 화면 밝게
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
            StartCoroutine(FadeIn());
        }
    }

    void OnCutsceneEnd(PlayableDirector obj)
    {
        // 현재 씬 이름 확인
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "YR2")
        {
            // 씬 이동 없이 오브젝트들 활성화
            foreach (var go in objectsToActivate)
            {
                if (go != null) go.SetActive(true);
            }
            Debug.Log("▶ YR2 컷씬 종료 → 오브젝트들 활성화 완료");
        }
        else
        {
            // 기본 동작: 씬 이동
            StartCoroutine(FadeOutAndLoadScene());
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }

    IEnumerator FadeOutAndLoadScene()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t / fadeDuration;
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        // 새 씬 들어가자마자 페이드 인
        StartCoroutine(FadeIn());
    }
}
