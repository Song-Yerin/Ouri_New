using Controller;
using UnityEngine;

[RequireComponent(typeof(CreatureMover))]
[RequireComponent(typeof(AutoWalk))]
public class CutsceneController : MonoBehaviour
{
    private CreatureMover mover;
    private AutoWalk auto;

    void Awake()
    {
        mover = GetComponent<CreatureMover>();
        auto = GetComponent<AutoWalk>();

        // 평소에는 수동 조작 ↔ AutoWalk OFF
        mover.enabled = true;
        auto.enabled = false;
    }

    // ── 타임라인 Signal에서 호출 ───────────────────
    public void BeginCutscene()   // AutoWalk ON
    {
        mover.enabled = false;
        auto.enabled = true;
    }

    public void EndCutscene()     // 원래 조작 복귀
    {
        auto.enabled = false;
        mover.enabled = true;
    }
}

