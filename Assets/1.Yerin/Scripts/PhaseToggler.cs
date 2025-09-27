using UnityEngine;

public class PhaseToggler : MonoBehaviour
{
    [Header("Phase Roots (컨테이너 GameObject)")]
    [SerializeField] private GameObject phase1Root; // GameObject_phase1
    [SerializeField] private GameObject phase2Root; // GameObject_phase2

    [Header("선택 옵션")]
    [SerializeField] private bool logDebug = false;

    // ----------- 기본 전환 -----------
    public void SwitchToPhase1()
    {
        if (logDebug) Debug.Log("[PhaseToggler] → Phase 1");
        SetActiveSafe(phase2Root, false);
        SetActiveSafe(phase1Root, true);
    }

    public void SwitchToPhase2()
    {
        if (logDebug) Debug.Log("[PhaseToggler] → Phase 2");
        SetActiveSafe(phase1Root, false);
        SetActiveSafe(phase2Root, true);
    }

    // ----------- onPhaseEndSuccess(int nextIndex) 같은 이벤트용 -----------
    // nextIndex가 1이면 페이즈1, 2면 페이즈2로 전환
    public void SwitchToPhase(int nextIndex)
    {
        if (nextIndex <= 1) SwitchToPhase1();
        else SwitchToPhase2();
    }

    // ----------- 에디터에서 테스트 버튼 -----------
    [ContextMenu("Test → Phase 1")]
    private void _TestP1() => SwitchToPhase1();

    [ContextMenu("Test → Phase 2")]
    private void _TestP2() => SwitchToPhase2();

    private static void SetActiveSafe(GameObject go, bool on)
    {
        if (!go) return;
        if (go.activeSelf != on) go.SetActive(on);
    }
}
