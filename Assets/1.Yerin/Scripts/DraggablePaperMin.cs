// DraggablePaperMin.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public class DraggablePaperMin : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public Image image;
    public RectTransform workArea;

    RectTransform rect;
    Canvas canvas;
    CanvasGroup cg;
    Camera eventCam;
    Vector2 offset;

    public void Init(Sprite sprite)
    {
        image.sprite = sprite;
    }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCam = canvas.worldCamera;

        if (!workArea)
        {
            var tag = transform.root.GetComponentInChildren<WorkAreaTag>(true);
            if (tag) workArea = tag.GetComponent<RectTransform>();
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        transform.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        cg.blocksRaycasts = false;
        if (workArea) transform.SetParent(workArea, true);
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, eventCam, out var local);
        offset = (Vector2)rect.localPosition - local;
        Debug.Log("[Paper] BeginDrag");
    }

    public void OnDrag(PointerEventData e)
    {
        if (!workArea) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(workArea, e.position, eventCam, out var p))
            rect.anchoredPosition = p + offset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        cg.blocksRaycasts = true;
    }
}
