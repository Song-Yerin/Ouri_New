using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInOnly : MonoBehaviour
{
    public CanvasGroup fadeCanvas;   // ���� �г�(CanvasGroup ���� ��)
    public float fadeDuration = 1f;  // ������ ������ų� ��ο����� �ð�(��)

    private Coroutine currentRoutine;

    void Start()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f; // ������ ������ ��Ӱ�
            FadeIn();              // �ڵ����� ���̵� �� ����
        }
    }

    // �ܺο��� ������ �� �ִ� FadeIn
    public void FadeIn()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(1f, 0f));
    }

    // �ܺο��� ������ �� �ִ� FadeOut
    public void FadeOut()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, 1f));
    }

    // ���� �ڷ�ƾ (from �� to �� alpha ����)
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
