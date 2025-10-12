using UnityEngine;
using UnityEngine.SceneManagement;

public class UIVisibilityController : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] string[] hideInScenes;    // 이 씬들에선 기본 숨김
    [SerializeField] float fade = 0.15f;

    void Reset() { cg = GetComponentInChildren<CanvasGroup>(true); }
    void Awake() { SceneManager.sceneLoaded += OnScene; }
    void OnDestroy() { SceneManager.sceneLoaded -= OnScene; }

    void OnScene(Scene s, LoadSceneMode m)
    {
        bool shouldHide = System.Array.Exists(hideInScenes, n => n == s.name);
        SetVisible(!shouldHide);
    }

    public void OnCutsceneStart() { SetVisible(false); }   // 타임라인 시그널에서 호출
    public void OnCutsceneEnd() { SetVisible(true); }      // 타임라인 시그널에서 호출

    public void SetVisible(bool v)
    {
        if (!cg) return;
        StopAllCoroutines();
        StartCoroutine(FadeTo(v ? 1f : 0f));
        cg.blocksRaycasts = v; cg.interactable = v;
    }
    System.Collections.IEnumerator FadeTo(float target)
    {
        float t = 0, start = cg.alpha;
        while (t < fade) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(start, target, t / fade); yield return null; }
        cg.alpha = target;
    }
}
