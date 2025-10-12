using UnityEngine;
using TMPro;
using DG.Tweening;

public class QuestBannerTween : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform banner;       // QuestBanner RectTransform
    [SerializeField] private CanvasGroup canvasGroup;    // QuestBanner CanvasGroup
    [SerializeField] private TextMeshProUGUI chapterText; // "Ex" 또는 챕터 같은 소제목
    [SerializeField] private TextMeshProUGUI titleText;   // "검의 시련" 같은 메인 타이틀

    [Header("Motion")]
    [SerializeField] private float enterY = 0f;          // 들어와서 멈출 위치
    [SerializeField] private float offscreenTopY = 600f; // 시작(화면 위)
    [SerializeField] private float offscreenBottomY = -600f; // 퇴장(화면 아래)
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

    void Reset()
    {
        // 자동 연결 편의
        banner = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (transform.Find("ChapterText")) chapterText = transform.Find("ChapterText").GetComponent<TextMeshProUGUI>();
        if (transform.Find("TitleText")) titleText = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
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
    }

    /// <summary>
    /// 타임라인 시그널/스크립트에서 호출: 배너를 보여주고 자동 퇴장한다.
    /// </summary>
    public void ShowBanner(string chapter, string title)
    {
        if (chapterText) chapterText.text = chapter;
        if (titleText) titleText.text = title;

        if (seq != null && seq.IsActive()) seq.Kill();

        // 시작 상태
        canvasGroup.alpha = 0f;
        banner.anchoredPosition = new Vector2(0f, offscreenTopY);

        // 시퀀스
        seq = DOTween.Sequence();

        // 진입
        seq.Append(canvasGroup.DOFade(1f, inDuration));
        seq.Join(banner.DOAnchorPosY(enterY, inDuration).SetEase(inEase)
                 .OnStart(() => {
                     PlaySfx(whooshIn);
                     PlaySfx(showJingle);
                 })
        );

        // 유지
        seq.AppendInterval(holdDuration);

        // 퇴장
        seq.Append(canvasGroup.DOFade(0f, outDuration));
        seq.Join(banner.DOAnchorPosY(offscreenBottomY, outDuration).SetEase(outEase)
                 .OnStart(() => PlaySfx(whooshOut))
        );

        // 종료 후 즉시 위쪽 대기 상태로 복귀시킬지 선택(취향)
        seq.OnComplete(() => {
            banner.anchoredPosition = new Vector2(0f, offscreenTopY);
        });
    }

    // 타임라인에서 파라미터 없이 호출하고 싶을 때 쓰는 편의 메서드
    public void ShowDefault()
    {
        ShowBanner("Ex", "검의 시련");
    }

    private void PlaySfx(AudioClip clip)
    {
        if (!sfx || !clip) return;
        sfx.PlayOneShot(clip);
    }
}
