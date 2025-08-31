using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorkAreaToggle : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] RectTransform workArea;      // 우상단 WorkArea 루트 (패널)
    [SerializeField] CanvasGroup cg;            // 없으면 자동으로 붙임
    [SerializeField] bool startHidden = true;     // 시작을 숨긴 상태로
    [SerializeField] bool bringToFrontOnOpen = true;

    [Header("Anim")]
    [SerializeField] float fadeDuration = 0.15f;  // 열고/닫기 시간
    [SerializeField] Vector3 openScale = Vector3.one;
    [SerializeField] Vector3 closeScale = new Vector3(0.96f, 0.96f, 1f);

    Coroutine routine;

    void Reset()
    {
        if (!workArea)
        {
            // 같은 오브젝트에 붙였다면 자동 탐색 시도
            workArea = GetComponent<RectTransform>();
        }
    }

    void Awake()
    {
        if (!workArea)
        {
            Debug.LogError("[WorkAreaToggle] workArea 레퍼런스가 비어있어요.");
            enabled = false;
            return;
        }

        if (!cg)
        {
            cg = workArea.GetComponent<CanvasGroup>();
            if (!cg) cg = workArea.gameObject.AddComponent<CanvasGroup>();
        }

        if (startHidden) SetHiddenImmediate();
        else SetShownImmediate();
    }

    // Button.onClick 에 이걸 연결하면 됨
    public void Toggle()
    {
        if (IsOpen()) Close();
        else Open();
    }

    public void Open()
    {
        if (routine != null) StopCoroutine(routine);
        if (bringToFrontOnOpen) workArea.SetAsLastSibling();
        workArea.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;
        routine = StartCoroutine(FadeTo(1f, openScale));
    }

    public void Close()
    {
        if (routine != null) StopCoroutine(routine);
        cg.interactable = false;
        cg.blocksRaycasts = false;
        routine = StartCoroutine(FadeTo(0f, closeScale, deactivateAtEnd: true));
    }

    public void EnsureOpen() { if (!IsOpen()) Open(); } // 다른 스크립트에서 호출용

    bool IsOpen() => workArea.gameObject.activeSelf && cg.alpha > 0.5f;

    void SetHiddenImmediate()
    {
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;
        workArea.localScale = closeScale;
        workArea.gameObject.SetActive(false);
    }
    void SetShownImmediate()
    {
        workArea.gameObject.SetActive(true);
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;
        workArea.localScale = openScale;
    }

    IEnumerator FadeTo(float targetAlpha, Vector3 targetScale, bool deactivateAtEnd = false)
    {
        float t = 0f;
        float startA = cg.alpha;
        Vector3 startS = workArea.localScale;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = t / fadeDuration; u = 1f - (1f - u) * (1f - u); // easeOut
            cg.alpha = Mathf.Lerp(startA, targetAlpha, u);
            workArea.localScale = Vector3.Lerp(startS, targetScale, u);
            yield return null;
        }
        cg.alpha = targetAlpha;
        workArea.localScale = targetScale;

        if (deactivateAtEnd) workArea.gameObject.SetActive(false);
        routine = null;
    }
}
