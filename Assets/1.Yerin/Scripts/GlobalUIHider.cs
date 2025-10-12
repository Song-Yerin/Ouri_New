using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExcludeFromGlobalUIHide : MonoBehaviour { } // 붙어있으면 전역 숨김에서 제외-다.

public class GlobalUIHider : MonoBehaviour
{
    [SerializeField] float fade = 0.15f;

    struct Saved
    {
        public float alpha; public bool blocks; public bool interact;
    }

    readonly Dictionary<CanvasGroup, Saved> saved = new();
    readonly Dictionary<CanvasGroup, Coroutine> running = new();

    public void CutsceneStart()
    {
        Debug.Log("[GlobalUIHider] CutsceneStart signal received");
        SetAll(false);
    }

    public void CutsceneEnd()
    {
        Debug.Log("[GlobalUIHider] CutsceneEnd signal received");
        SetAll(true);
    }

    void SetAll(bool visible)
    {
        // 1) 모든 Canvas 찾기 (비활성 포함, DontDestroyOnLoad 포함)
        var canvases = Object.FindObjectsOfType<Canvas>(true);

        // 2) CanvasGroup 확보 (없으면 임시로 부착)
        List<CanvasGroup> groups = new(canvases.Length);
        foreach (var c in canvases)
        {
            if (c.GetComponentInParent<ExcludeFromGlobalUIHide>(true)) continue;

            var g = c.GetComponent<CanvasGroup>();
            if (!g) g = c.gameObject.AddComponent<CanvasGroup>(); // 없는 UI도 제어-다.
            groups.Add(g);
        }

        // 3) 일괄 페이드
        foreach (var g in groups)
        {
            if (!saved.ContainsKey(g))
            {
                saved[g] = new Saved { alpha = g.alpha, blocks = g.blocksRaycasts, interact = g.interactable };
            }
            if (running.TryGetValue(g, out var co) && co != null) StopCoroutine(co);
            running[g] = StartCoroutine(FadeTo(g, visible ? saved[g].alpha : 0f, visible));
        }
    }

    IEnumerator FadeTo(CanvasGroup g, float target, bool visible)
    {
        float t = 0f, start = g.alpha;
        // 입력 차단 즉시 반영
        g.blocksRaycasts = visible ? saved[g].blocks : false;
        g.interactable = visible ? saved[g].interact : false;

        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(start, target, t / fade);
            yield return null;
        }
        g.alpha = target;
    }
}
