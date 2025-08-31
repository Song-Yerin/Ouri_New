using UnityEngine;
using UnityEngine.UI;

public class MinimapMarker : MonoBehaviour
{
    public Transform target;          // ������ ������Ʈ (����/NPC)
    public RectTransform markerUI;    // ������(UI)
    public Camera minimapCamera;      // �̴ϸ� ī�޶�
    public RectTransform minimapUI;   // RawImage (RectTransform)

    public float blinkSpeed = 2f;
    private Image markerImage;

    void Start()
    {
        markerImage = markerUI.GetComponent<Image>();
    }

    void Update()
    {
        // ���� �� ����Ʈ (0~1)
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(target.position);

        // ����Ʈ �� UI localPosition
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
