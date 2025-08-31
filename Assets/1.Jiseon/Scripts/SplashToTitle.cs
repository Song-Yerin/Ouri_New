using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplashToTitle : MonoBehaviour
{
    [Header("스플래시 사용 여부")]
    [Tooltip("체크 해제하면 스플래시 없이 바로 카메라 조작이 활성화됩니다.")]
    public bool enableSplash = true;

    // 검은 배경 패널(CanvasGroup)
    public CanvasGroup panelGroup;
    // 로고 이미지(Image)
    public Image splash;

    // SmoothMouseLook 컴포넌트 참조
    public SmoothMouseLook smoothMouseLook;

    // 이미지 페이드 인/아웃 시간
    public float fadeDuration = 1f;
    // 이미지 표시 시간
    public float imageDisplay = 2f;

    void Awake()
    {
        if (!enableSplash)
        {
            // 스플래시 스킵: 패널·로고 숨기고 곧바로 마우스 룩 활성화
            if (smoothMouseLook != null)
                smoothMouseLook.enabled = true;

            panelGroup.alpha = 0f;
            var c = splash.color;
            c.a = 0f;
            splash.color = c;
            return;
        }

        // enableSplash == true 인 경우, 그룹이 꺼져 있으면 켠다
        if (!panelGroup.gameObject.activeInHierarchy)
            panelGroup.gameObject.SetActive(true);
        if (!splash.gameObject.activeInHierarchy)
            splash.gameObject.SetActive(true);

        // 스플래시 사용: 초기 상태 세팅
        panelGroup.alpha = 1f;
        var col = splash.color;
        col.a = 0f;
        splash.color = col;

        if (smoothMouseLook != null)
            smoothMouseLook.enabled = false;
    }

    IEnumerator Start()
    {
        // 스플래시를 껐다면 이 코루틴 자체를 종료
        if (!enableSplash)
            yield break;

        yield return new WaitForSeconds(1f);

        // 1. 이미지 페이드 인
        yield return StartCoroutine(FadeImage(splash, 0f, 1f));

        // 2. 이미지 표시 대기
        yield return new WaitForSeconds(imageDisplay);

        // 3. 이미지 페이드 아웃
        yield return StartCoroutine(FadeImage(splash, 1f, 0f));

        yield return new WaitForSeconds(1f);

        // 4. 패널 페이드 아웃
        yield return StartCoroutine(FadeCanvas(panelGroup, 1f, 0f));

        // 5. 패널 사라진 뒤, SmoothMouseLook 활성화
        if (smoothMouseLook != null)
            smoothMouseLook.enabled = true;

    }

    // CanvasGroup alpha를 부드럽게 from → to 변경
    IEnumerator FadeCanvas(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }

    // Image color.a를 부드럽게 from → to 변경
    IEnumerator FadeImage(Image img, float from, float to)
    {
        float elapsed = 0f;
        var col = img.color;
        col.a = from;
        img.color = col;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            col = img.color;
            col.a = a;
            img.color = col;
            yield return null;
        }

        col = img.color;
        col.a = to;
        img.color = col;
    }
}
