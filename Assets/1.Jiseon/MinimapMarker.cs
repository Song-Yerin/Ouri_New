using UnityEngine;
using UnityEngine.UI;

public class MinimapMarker : MonoBehaviour
{
    public Transform target;          // 추적할 오브젝트 (보물/NPC)
    public RectTransform markerUI;    // 아이콘(UI)
    public Camera minimapCamera;      // 미니맵 카메라
    public RectTransform minimapUI;   // RawImage (RectTransform)

    public float blinkSpeed = 2f;
    private Image markerImage;

    void Start()
    {
        markerImage = markerUI.GetComponent<Image>();
    }

    void Update()
    {
        // 월드 → 뷰포트 (0~1)
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(target.position);

        // 뷰포트 → UI localPosition
        Vector2 minimapSize = minimapUI.rect.size;
        Vector2 uiPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapSize.x,
            (viewportPos.y - 0.5f) * minimapSize.y
        );

        markerUI.localPosition = uiPos;

        Color c = markerImage.color;
        markerImage.color = c;
    }
}
