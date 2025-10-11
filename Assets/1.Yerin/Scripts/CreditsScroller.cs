using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll")]
    public RectTransform content;     // Content
    public RectTransform viewport;    // Viewport (마스크)
    public float speed = 80f;         // px/sec
    public float startOffset = -200f; // 시작 위치(아래로 조금 숨김)
    public float endPadding = 200f;   // 끝난 뒤 여유 거리

    [Header("Control")]
    public float fastRate = 3f;       // 누르면 빨라지는 배수
    public KeyCode skipKey = KeyCode.Escape;
    public string nextSceneName = ""; // 비워두면 아무 것도 안 함

    [Header("Fade")]
    public CanvasGroup fadeGroup;     // 선택(있으면 페이드)
    public float fadeInTime = 0.75f;
    public float fadeOutTime = 0.75f;

    float _yStart, _yEnd;
    bool _running;

    void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        // 시작/끝 y 계산
        _yStart = -viewport.rect.height / 2f - startOffset;
        float contentHalf = content.rect.height / 2f;
        _yEnd = contentHalf + viewport.rect.height / 2f + endPadding;

        // 시작 위치 세팅
        var p = content.anchoredPosition;
        p.y = _yStart;
        content.anchoredPosition = p;

        if (fadeGroup) { fadeGroup.alpha = 0; StartCoroutine(Fade(1, fadeInTime)); }
        _running = true;
    }

    void Update()
    {
        if (!_running) return;

        // 입력
        float rate = (Input.anyKey && !Input.GetKey(skipKey)) ? fastRate : 1f;
        if (Input.GetKeyDown(skipKey)) { EndCredits(); return; }

        // 스크롤
        var p = content.anchoredPosition;
        p.y += speed * rate * Time.deltaTime;
        content.anchoredPosition = p;

        // 끝난 경우
        if (p.y >= _yEnd) EndCredits();
    }

    void EndCredits()
    {
        _running = false;
        if (fadeGroup) StartCoroutine(Fade(0, fadeOutTime, () => LoadNext()));
        else LoadNext();
    }

    System.Collections.IEnumerator Fade(float target, float t, System.Action onDone = null)
    {
        float s = fadeGroup.alpha, e = target, elapsed = 0f;
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(s, e, elapsed / t);
            yield return null;
        }
        fadeGroup.alpha = e;
        onDone?.Invoke();
    }

    void LoadNext()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
