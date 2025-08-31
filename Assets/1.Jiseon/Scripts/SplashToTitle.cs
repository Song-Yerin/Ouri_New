using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplashToTitle : MonoBehaviour
{
    [Header("���÷��� ��� ����")]
    [Tooltip("üũ �����ϸ� ���÷��� ���� �ٷ� ī�޶� ������ Ȱ��ȭ�˴ϴ�.")]
    public bool enableSplash = true;

    // ���� ��� �г�(CanvasGroup)
    public CanvasGroup panelGroup;
    // �ΰ� �̹���(Image)
    public Image splash;

    // SmoothMouseLook ������Ʈ ����
    public SmoothMouseLook smoothMouseLook;

    // �̹��� ���̵� ��/�ƿ� �ð�
    public float fadeDuration = 1f;
    // �̹��� ǥ�� �ð�
    public float imageDisplay = 2f;

    void Awake()
    {
        if (!enableSplash)
        {
            // ���÷��� ��ŵ: �гΡ��ΰ� ����� ��ٷ� ���콺 �� Ȱ��ȭ
            if (smoothMouseLook != null)
                smoothMouseLook.enabled = true;

            panelGroup.alpha = 0f;
            var c = splash.color;
            c.a = 0f;
            splash.color = c;
            return;
        }

        // enableSplash == true �� ���, �׷��� ���� ������ �Ҵ�
        if (!panelGroup.gameObject.activeInHierarchy)
            panelGroup.gameObject.SetActive(true);
        if (!splash.gameObject.activeInHierarchy)
            splash.gameObject.SetActive(true);

        // ���÷��� ���: �ʱ� ���� ����
        panelGroup.alpha = 1f;
        var col = splash.color;
        col.a = 0f;
        splash.color = col;

        if (smoothMouseLook != null)
            smoothMouseLook.enabled = false;
    }

    IEnumerator Start()
    {
        // ���÷��ø� ���ٸ� �� �ڷ�ƾ ��ü�� ����
        if (!enableSplash)
            yield break;

        yield return new WaitForSeconds(1f);

        // 1. �̹��� ���̵� ��
        yield return StartCoroutine(FadeImage(splash, 0f, 1f));

        // 2. �̹��� ǥ�� ���
        yield return new WaitForSeconds(imageDisplay);

        // 3. �̹��� ���̵� �ƿ�
        yield return StartCoroutine(FadeImage(splash, 1f, 0f));

        yield return new WaitForSeconds(1f);

        // 4. �г� ���̵� �ƿ�
        yield return StartCoroutine(FadeCanvas(panelGroup, 1f, 0f));

        // 5. �г� ����� ��, SmoothMouseLook Ȱ��ȭ
        if (smoothMouseLook != null)
            smoothMouseLook.enabled = true;

    }

    // CanvasGroup alpha�� �ε巴�� from �� to ����
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

    // Image color.a�� �ε巴�� from �� to ����
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
