using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class QuestBannerTween : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform banner;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI chapterText;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Motion")]
    [SerializeField] private float enterY = 0f;
    [SerializeField] private float offscreenTopY = 600f;
    [SerializeField] private float offscreenBottomY = -600f;
    [SerializeField] private float inDuration = 0.6f;
    [SerializeField] private float holdDuration = 1.4f;
    [SerializeField] private float outDuration = 0.6f;
    [SerializeField] private Ease inEase = Ease.OutCubic;
    [SerializeField] private Ease outEase = Ease.InCubic;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip showJingle;
    [SerializeField] private AudioClip whooshIn;
    [SerializeField] private AudioClip whooshOut;

    private Sequence seq;
    private bool isPlaying = false;

    // 세션 전역: 이미 표시한 씬 이름
    private static readonly HashSet<string> shownScenes = new();

    public void ShowBannerOnce(string chapter, string title)
    {
        var scene = SceneManager.GetActiveScene().name;

        // 전역 가드
        if (!shownScenes.Add(scene))
        {
            Debug.Log($"[Banner] Skip (already shown in scene: {scene})", this);
            return;
        }

        // 로컬/재생중 가드
        if (isPlaying)
        {
            Debug.Log($"[Banner] Skip (isPlaying) by {name}", this);
            return;
        }

        Debug.Log($"[Banner] PLAY once in scene '{scene}' by {name}", this);
        ShowBanner_Internal(chapter, title);
    }

    // ⛔ 외부 직접 호출 금지: private
    private void ShowBanner_Internal(string chapter, string title)
    {
        isPlaying = true;

        if (chapterText) chapterText.text = chapter;
        if (titleText) titleText.text = title;

        if (seq != null && seq.IsActive()) seq.Kill();

        canvasGroup.alpha = 0f;
        banner.anchoredPosition = new Vector2(0f, offscreenTopY);

        seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, inDuration));
        seq.Join(banner.DOAnchorPosY(enterY, inDuration).SetEase(inEase)
            .OnStart(() => { PlaySfx(whooshIn); PlaySfx(showJingle); })
        );
        seq.AppendInterval(holdDuration);
        seq.Append(canvasGroup.DOFade(0f, outDuration));
        seq.Join(banner.DOAnchorPosY(offscreenBottomY, outDuration).SetEase(outEase)
            .OnStart(() => PlaySfx(whooshOut))
        );
        seq.OnComplete(() =>
        {
            banner.anchoredPosition = new Vector2(0f, offscreenTopY);
            isPlaying = false;
        });
    }

    void Reset()
    {
        banner = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!chapterText && transform.Find("ChapterText"))
            chapterText = transform.Find("ChapterText").GetComponent<TextMeshProUGUI>();
        if (!titleText && transform.Find("TitleText"))
            titleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
    }
    void OnValidate()
    {
        if (!banner) banner = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }
    void Awake()
    {
        if (!banner) banner = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        HideImmediate();
    }
    public void HideImmediate()
    {
        if (seq != null && seq.IsActive()) seq.Kill();
        canvasGroup.alpha = 0f;
        banner.anchoredPosition = new Vector2(0f, offscreenTopY);
        isPlaying = false;
    }
    private void PlaySfx(AudioClip clip)
    {
        if (!sfx || !clip) return;
        sfx.PlayOneShot(clip);
    }

    public void ShowBanner(string chapter, string title)
    {
        ShowBannerOnce(chapter, title);
    }

    // 새 게임/타이틀 복귀 시 필요하면 호출
    public static void ResetAllShown() => shownScenes.Clear();

    // 에디터에서 Domain Reload 꺼놨을 때 세션 시작마다 클리어하고 싶으면 주석 해제
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //static void ClearOnPlayStart() => shownScenes.Clear();
}
