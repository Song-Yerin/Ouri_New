using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public int acceptPieceIndex;
    public PaperData targetData;
    [SerializeField] RectTransform piecesRoot;

    public PuzzlePanelUI owner;

    PaperPieceMeta current;
    RectTransform rect;
    Canvas canvas;

    [Header("Snap FX")]
    [SerializeField] float snapDuration = 0.12f;
    [SerializeField] float bounceScale = 1.08f;

    // ★ 진행 중 스냅 코루틴 핸들(중복 방지/파괴시 정리)
    Coroutine _snapCR;

    void Awake()
    {
        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
    }

    void OnDisable() { StopAllCoroutines(); _snapCR = null; }
    void OnDestroy() { StopAllCoroutines(); _snapCR = null; }

    public void SetPiecesRoot(RectTransform root) => piecesRoot = root;

    public void ClearVisualOnly() { current = null; }
    public void ClearAndUnplace()
    {
        if (current != null)
            PaperInventory.Instance?.SetPiecePlaced(current.data, current.pieceIndex, false);
        current = null;
    }

    public void OnDrop(PointerEventData e)
    {
        var go = e.pointerDrag;
        if (!go || !piecesRoot || !targetData) return;

        var meta = go.GetComponent<PaperPieceMeta>();
        if (!meta || meta.data != targetData) return;

        StartSnap((RectTransform)go.transform, meta);
    }

    public void SnapFromScript(RectTransform piece, PaperPieceMeta meta)
    {
        if (!piece || !meta || !piecesRoot) return;
        if (meta.data != targetData) return;
        StartSnap(piece, meta);
    }

    // ★ 스냅 시작은 항상 여기로(중복 실행 방지)
    void StartSnap(RectTransform piece, PaperPieceMeta meta)
    {
        if (_snapCR != null) StopCoroutine(_snapCR);
        _snapCR = StartCoroutine(SnapTween(piece, meta));
    }

    IEnumerator OwnerDeferredCheck(PuzzlePanelUI panel, PaperData data)
    {
        yield return null;
        if (panel) panel.CheckCompletion(data);
    }

    IEnumerator SnapTween(RectTransform piece, PaperPieceMeta meta)
    {
        // 초기 가드
        if (!this || !gameObject.activeInHierarchy) yield break;
        if (!piece || !piecesRoot || !rect) yield break;

        // 부모 고정 (파괴/비활성 가드)
        if (piece) piece.SetParent(piecesRoot, false);
        else yield break;

        // 슬롯 중심 → piecesRoot 좌표
        var cam = canvas ? canvas.worldCamera : null;
        Vector2 endPos;
        {
            // rect 또는 piecesRoot가 중간에 사라질 수 있으므로 try
            if (!rect || !piecesRoot) yield break;
            var screen = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(piecesRoot, screen, cam, out endPos))
                yield break;
        }

        // 트윈
        var startPos = piece ? piece.anchoredPosition : Vector2.zero;
        var baseScale = piece ? piece.localScale : Vector3.one;
        float t = 0f;

        var cg = piece ? piece.GetComponent<CanvasGroup>() : null;
        if (cg) cg.blocksRaycasts = false;

        while (t < snapDuration)
        {
            // ★ 루프 가드: 대상/부모/슬롯/오너가 살아있는지 계속 확인
            if (!this || !gameObject.activeInHierarchy) yield break;
            if (!piece || !piecesRoot || !rect) yield break;

            t += Time.unscaledDeltaTime;
            float u = t / snapDuration; u = 1f - (1f - u) * (1f - u);

            // 일부 프레임에서 파괴되면 예외가 날 수 있어 try 보호
            try
            {
                piece.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, u);
            }
            catch { yield break; }

            yield return null;
        }

        if (piece) piece.anchoredPosition = endPos; else yield break;

        // 살짝 튕김
        float bt = 0f, bd = 0.08f;
        if (piece) piece.localScale = baseScale * bounceScale; else yield break;
        while (bt < bd)
        {
            if (!this || !gameObject.activeInHierarchy) yield break;
            if (!piece) yield break;

            bt += Time.unscaledDeltaTime;
            float u = bt / bd; u = 1f - (1f - u) * (1f - u);
            try
            {
                piece.localScale = Vector3.LerpUnclamped(baseScale * bounceScale, baseScale, u);
            }
            catch { yield break; }

            yield return null;
        }
        if (piece) piece.localScale = baseScale;

        // 이전 슬롯 점유 해제
        if (meta && meta.currentSlot && meta.currentSlot != this)
            meta.currentSlot.ClearVisualOnly();

        // 현재 슬롯으로 등록
        if (meta)
        {
            meta.currentSlot = this;
            current = meta;

            PaperInventory.Instance?.SetPiecePlacedInSlot(meta.data, meta.pieceIndex, acceptPieceIndex);
            PaperInventory.Instance?.SetPiecePlaced(meta.data, meta.pieceIndex, true);
        }

        if (cg) cg.blocksRaycasts = true;

        // 완성 체크
        var o = owner;
        var d = targetData;
        o?.CheckCompletion(d);
        if (o != null && o.isActiveAndEnabled)
            o.StartCoroutine(OwnerDeferredCheck(o, d));

        _snapCR = null;
    }

    public bool HasCorrectPiece() => current && current.pieceIndex == acceptPieceIndex;
}
