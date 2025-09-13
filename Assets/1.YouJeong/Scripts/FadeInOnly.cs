using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInOnly : MonoBehaviour
{
    public CanvasGroup fadeCanvas;   // 검은 패널(CanvasGroup 붙은 것)
    public float fadeDuration = 1f;  // 서서히 밝아지거나 어두워지는 시간(초)

    private Coroutine currentRoutine;

    void Start()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f; // 시작은 완전히 어둡게
            FadeIn();              // 자동으로 페이드 인 시작
        }
    }

    // 외부에서 실행할 수 있는 FadeIn
    public void FadeIn()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(1f, 0f));
    }

    // 외부에서 실행할 수 있는 FadeOut
    public void FadeOut()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, 1f));
    }

    // 공용 코루틴 (from → to 로 alpha 변경)
    IEnumerator FadeRoutine(float from, float to)
    {
        float t = 0f;
        fadeCanvas.alpha = from;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
        currentRoutine = null;
    }
}
