using UnityEngine;
using UnityEngine.UI;

public class FadeOnSignal : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;         // 검은색 UI 이미지 (전체화면용)
    public float duration = 1f;     // 페이드 시간 (초)
    public bool fadeIn = true;      // true면 점점 검게, false면 점점 밝게
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine _running;

    // 공통 실행 함수
    private void StartFade()
    {
        if (!fadeImage)
        {
            Debug.LogWarning("Fade Image가 지정되지 않았습니다.");
            return;
        }

        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeRoutine());
    }

    // 타임라인 시그널에서 직접 호출
    public void OnSignalFadeIn()
    {
        fadeIn = true;  // 검게
        StartFade();
    }

    public void OnSignalFadeOut()
    {
        fadeIn = false; // 밝게
        StartFade();
    }

    System.Collections.IEnumerator FadeRoutine()
    {
        Color color = fadeImage.color;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, duration);
            float k = easing.Evaluate(Mathf.Clamp01(t));
            color.a = Mathf.Lerp(startAlpha, endAlpha, k);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
        _running = null;
    }
}

