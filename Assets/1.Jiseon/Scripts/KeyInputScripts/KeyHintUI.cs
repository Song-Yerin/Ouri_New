using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KeyHintUI : MonoBehaviour
{
    public Image keyImage;
    public TMP_Text description;
    public KeyCode watchKey = KeyCode.None;

    private bool isFadingOut = false;
    private float fadeDuration = 1.0f; // 페이드 시간
    private bool checkKeyInput = false;

    public void Init(Sprite keySprite, string message, float duration)
    {
        keyImage.sprite = keySprite;
        description.text = message;
        gameObject.SetActive(true);

        // 처음 알파값 0으로 시작해서 페이드 인
        SetAlpha(0f);
        StartCoroutine(FadeIn());

        // 키 입력 감지 여부
        checkKeyInput = duration > 0f;

        if (duration > 0)
            StartCoroutine(HideAfterSeconds(duration));
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!isFadingOut)
            StartCoroutine(FadeOutAndDestroy());
    }

    private void Update()
    {
        if (checkKeyInput && !isFadingOut && watchKey != KeyCode.None && Input.GetKeyDown(watchKey))
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    public void NotifyMissionComplete()
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(1f);
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;

        float elapsed = 0f;
        Color keyColor = keyImage.color;
        Color textColor = description.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (keyImage != null)
        {
            Color c = keyImage.color;
            keyImage.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (description != null)
        {
            Color c = description.color;
            description.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}
