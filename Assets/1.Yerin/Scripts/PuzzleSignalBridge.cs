using UnityEngine;

public class PuzzleSignalBridge : MonoBehaviour
{
    [SerializeField] PuzzlePanelUI target;

    void Awake()
    {
        if (!target) target = FindObjectOfType<PuzzlePanelUI>(true);
    }

    public void HideOnce() => target?.HideCompletedPaperOnce();
    public void BeginHide() => target?.BeginHideCompletedPaper();
    public void EndHide() => target?.EndHideCompletedPaper();
    public void HideFor(float sec) => target?.HideCompletedPaperFor(sec);
}
