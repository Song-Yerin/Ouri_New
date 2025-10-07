using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathFaceEffect : MonoBehaviour
{
    [Header("이미지 설정")]
    public Image faceImage;           // 죽을 때 표시할 얼굴 이미지
    public Image fadePanel;           // 화면 어두워짐용 패널 (검정색 Image)

    [Header("크기 설정 (1단계 & 5단계 따로 조절 가능)")]
    [Tooltip("① 처음 시작할 때 얼굴 크기 (크게 시작할수록 임팩트 강함)")]
    public float startScalePhase1 = 8f;     // ① 처음 시작할 때 얼굴 크기

    [Tooltip("① 줄어든 후 얼굴 크기 (최소 크기)")]
    public float endScalePhase1 = 1f;       // ① 줄어든 후 얼굴 크기

    [Tooltip("① 줄어드는 데 걸리는 시간")]
    public float shrinkDuration = 1.2f;     // ① 줄어드는 시간

    [Space(5)]
    [Tooltip("⑤ 마지막 커지기 시작할 때 얼굴 크기")]
    public float startScalePhase5 = 1f;     // ⑤ 마지막 커지기 시작 크기

    [Tooltip("⑤ 마지막 얼굴이 얼마나 커질지 (Inspector에서 조절 가능)")]
    public float endScalePhase5 = 6f;       // ⑤ 마지막 커질 때 얼굴 크기

    [Tooltip("⑤ 커지는 데 걸리는 시간")]
    public float growDuration = 1.2f;       // ⑤ 커지는 시간

    [Header("페이드 설정")]
    [Tooltip("화면이 어두워지는 속도 (페이드 인)")]
    public float fadeInDuration = 1f;

    [Tooltip("화면이 완전히 어두운 상태로 유지되는 시간")]
    public float fadeHoldTime = 0.5f;

    [Tooltip("화면이 다시 밝아지는 속도 (페이드 아웃)")]
    public float fadeOutDuration = 1.5f;

    [Tooltip("얼굴이 천천히 사라질 때 걸리는 시간")]
    public float faceFadeOutDuration = 1.5f;

    [Header("테스트용 키")]
    [Tooltip("이 키를 누르면 연출이 실행됩니다 (테스트용)")]
    public KeyCode triggerKey = KeyCode.K;

    private bool isPlaying = false;

    void Start()
    {
        // 초기화: 이미지 비활성화, 알파 초기값 설정
        if (faceImage != null)
        {
            faceImage.gameObject.SetActive(false);
            Color fc = faceImage.color;
            fc.a = 1f;
            faceImage.color = fc;
        }

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }
    }

    void Update()
    {
        // 테스트용: triggerKey 누르면 연출 실행
        if (Input.GetKeyDown(triggerKey) && !isPlaying)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        if (faceImage == null || fadePanel == null)
        {
            Debug.LogError("DeathFaceEffect: faceImage 또는 fadePanel이 설정되지 않았습니다!");
            yield break;
        }

        isPlaying = true;

        // 얼굴 표시 및 알파 복원 (이거 없으면 2번째 실행부터 안보임)
        faceImage.gameObject.SetActive(true);
        Color fc = faceImage.color;
        fc.a = 1f;
        faceImage.color = fc;

        // 얼굴 스케일 초기화
        faceImage.rectTransform.localScale = Vector3.one * startScalePhase1;

        // 1) 얼굴이 크게 시작 → 작아짐
        yield return StartCoroutine(ScaleFace(startScalePhase1, endScalePhase1, shrinkDuration));

        // 2) 화면 어두워짐 (페이드 인)
        yield return StartCoroutine(FadePanel(0f, 1f, fadeInDuration));

        // 3) 어두운 상태 유지
        yield return new WaitForSeconds(fadeHoldTime);

        // 4) 화면 밝아짐 (페이드 아웃)
        yield return StartCoroutine(FadePanel(1f, 0f, fadeOutDuration));

        // 5) 얼굴 작게 → 커짐
        faceImage.rectTransform.localScale = Vector3.one * startScalePhase5;
        yield return StartCoroutine(ScaleFace(startScalePhase5, endScalePhase5, growDuration));

        // 6) 얼굴이 서서히 사라짐 (알파값 1 → 0)
        yield return StartCoroutine(FadeFaceOut(1f, 0f, faceFadeOutDuration));

        // 7) 완전히 투명해진 후 비활성화
        faceImage.gameObject.SetActive(false);
        isPlaying = false;
    }

    private IEnumerator ScaleFace(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            float scale = Mathf.Lerp(from, to, t);
            faceImage.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        faceImage.rectTransform.localScale = Vector3.one * to;
    }

    private IEnumerator FadePanel(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadePanel.color = c;
            yield return null;
        }

        c.a = to;
        fadePanel.color = c;
    }

    private IEnumerator FadeFaceOut(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = faceImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            faceImage.color = c;
            yield return null;
        }

        c.a = to;
        faceImage.color = c;
    }
}
