using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), RequireComponent(typeof(CanvasGroup))]
public class UIDragClamp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform dragArea;     // pieceContainer 권장
    [SerializeField] float magnetRadius = 120f;  // 픽셀 단위
    [SerializeField] bool debugLogs = false;

    RectTransform rect;
    CanvasGroup cg;
    Canvas canvas;
    Vector2 pointerOffset;
    bool dragging;

    void Awake()
    {
        rect = (RectTransform)transform;
        cg = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetArea(RectTransform area) => dragArea = area;

    public void OnBeginDrag(PointerEventData e)
    {
        dragging = true;
        if (!dragArea) dragArea = rect.parent as RectTransform;
        if (cg) cg.blocksRaycasts = false;
        transform.SetAsLastSibling();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var local))
            pointerOffset = rect.anchoredPosition - local;
        else
            pointerOffset = Vector2.zero;

        if (debugLogs) Debug.Log("[Drag] Begin");

        var meta = GetComponent<PaperPieceMeta>();
        if (meta != null && meta.currentSlot != null)
        {
            var prev = meta.currentSlot;
            var prevOwner = prev.owner;     // owner를 먼저 잡아둔다
            meta.currentSlot = null;
            prev.ClearVisualOnly();
            PaperInventory.Instance?.SetPiecePlaced(meta.data, meta.pieceIndex, false);
            PaperInventory.Instance?.SetPiecePlacedInSlot(meta.data, meta.pieceIndex, -1);
            prevOwner?.CheckCompletion(meta.data);   // 이전 슬롯 비워졌다고 즉시 알림
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragArea) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, e.position, e.pressEventCamera, out var local)) return;

        var target = local + pointerOffset;

        var area = dragArea.rect;
        var size = rect.rect.size;
        float left = area.xMin + size.x * rect.pivot.x;
        float right = area.xMax - size.x * (1 - rect.pivot.x);
        float bottom = area.yMin + size.y * rect.pivot.y;
        float top = area.yMax - size.y * (1 - rect.pivot.y);
        target.x = Mathf.Clamp(target.x, left, right);
        target.y = Mathf.Clamp(target.y, bottom, top);

        rect.anchoredPosition = target;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (debugLogs) Debug.Log("[Drag] End");
        dragging = false;
        RestoreRaycast();
        TryMagnetSnap();               
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (debugLogs) Debug.Log("[Drag] PointerUp");
        dragging = false;
        RestoreRaycast();
        TryMagnetSnap();              
    }

    void OnDisable()
    {
        if (dragging && debugLogs) Debug.Log("[Drag] Disabled while dragging");
        dragging = false;
        RestoreRaycast();
        // 비활성 중에도 스냅 못했으면 남기지 않고 종료
    }

    void RestoreRaycast() { if (cg) cg.blocksRaycasts = true; }

    void TryMagnetSnap()
    {
        if (magnetRadius <= 0f) return;

        var meta = GetComponent<PaperPieceMeta>();
        if (!meta) return;

        var slots = GameObject.FindObjectsOfType<DropSlot>(true);
        DropSlot best = null; float bestDist = float.MaxValue;

        var cam = canvas ? canvas.worldCamera : null;
        Vector2 pieceScreen = RectTransformUtility.WorldToScreenPoint(cam, rect.position);

        foreach (var s in slots)
        {
            if (!s || s.targetData != meta.data) continue;        // ✅ 퍼즐만 같으면 후보

            var srt = (RectTransform)s.transform;
            Vector2 slotScreen = RectTransformUtility.WorldToScreenPoint(cam, srt.position);

            float d = Vector2.Distance(pieceScreen, slotScreen);
            if (d < bestDist) { bestDist = d; best = s; }
        }

        if (best != null && bestDist <= magnetRadius)
            best.SnapFromScript(rect, meta); // 슬롯이 자리 확정
    }

}


