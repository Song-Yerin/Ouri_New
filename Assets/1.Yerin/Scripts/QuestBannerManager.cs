using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestBannerManager : MonoBehaviour
{
    public static QuestBannerManager I { get; private set; }

    [Header("Refs")]
    [SerializeField] private QuestBannerTween banner;    // 배너 트윈 스크립트
    [SerializeField] private QuestBannerTable table;     // SO 데이터

    [Header("Behavior")]
    [SerializeField] private bool autoShowOnSceneStart = true;
    [SerializeField] private float autoDelay = 1.0f;     // 씬 시작 후 약간 늦게
    [SerializeField] private float cutsceneGrace = 3.0f; // 컷씬 신호 대기 시간

    bool alreadyShownThisScene;
    bool cutsceneWillShow;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        // 필요하면 전역 유지
        // DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scn, LoadSceneMode mode)
    {
        alreadyShownThisScene = false;
        cutsceneWillShow = false;
        if (autoShowOnSceneStart) StartCoroutine(Co_AutoShowFlow(scn.name));
    }

    IEnumerator Co_AutoShowFlow(string sceneName)
    {
        // 컷씬 신호 기다릴 여유(그레이스) → 신호가 오면 자동표시 취소
        float t = 0f;
        while (t < cutsceneGrace)
        {
            if (cutsceneWillShow) yield break; // 컷씬이 책임짐
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        // 컷씬 신호가 없으면 자동으로 띄움 (약간의 추가 지연)
        yield return new WaitForSecondsRealtime(autoDelay);
        ShowForScene(sceneName);
    }

    /// 타임라인 시그널에서 호출: 컷씬이 끝나는 타이밍에 호출
    public void ShowForCurrentSceneFromCutscene()
    {
        cutsceneWillShow = true; // 자동표시 취소
        ShowForScene(SceneManager.GetActiveScene().name);
    }

    /// 외부에서 특정 텍스트로 강제 표출하고 싶을 때
    public void ShowDirect(string chapter, string title)
    {
        if (alreadyShownThisScene) return;
        banner.ShowBanner(chapter, title);
        alreadyShownThisScene = true;
    }

    /// 씬의 SO 매핑을 찾아서 표시
    public void ShowForScene(string sceneName)
    {
        if (alreadyShownThisScene) return;
        if (table && table.TryGet(sceneName, out var e))
        {
            banner.ShowBanner(string.IsNullOrEmpty(e.chapter) ? "Ex" : e.chapter, e.title);
            alreadyShownThisScene = true;
        }
    }
}
