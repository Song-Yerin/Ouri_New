using UnityEngine;

public class CreditScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public RectTransform content;   // 크레딧 텍스트가 들어있는 Content
    public float speed = 60f;       // 초당 이동 속도(px/sec)
    public float startOffset = -200f; // 시작 Y위치 (살짝 아래에서 시작)
    public float endPadding = 200f;   // 끝난 뒤 여유 거리

    private float startY;
    private float endY;
    private bool scrolling = true;

    void OnEnable()
    {
        // 초기 위치 세팅
        startY = startOffset;
        endY = content.rect.height + endPadding;

        Vector2 startPos = content.anchoredPosition;
        startPos.y = startY;
        content.anchoredPosition = startPos;
    }

    void Update()
    {
        if (!scrolling) return;

        // 위로 이동
        Vector2 pos = content.anchoredPosition;
        pos.y += speed * Time.deltaTime;
        content.anchoredPosition = pos;

        // 다 올라가면 멈춤
        if (pos.y >= endY)
        {
            scrolling = false;
            Debug.Log("Credits finished!");
        }
    }
}
