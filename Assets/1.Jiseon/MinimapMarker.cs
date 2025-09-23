using UnityEngine;
using UnityEngine.UI;

public class MinimapMarker : MonoBehaviour
{
    public Transform target;
    public RectTransform markerUI;
    public Camera minimapCamera;
    public RectTransform minimapUI;

    public float blinkSpeed = 2f;
    private Image markerImage;

    void Start()
    {
        markerImage = markerUI.GetComponent<Image>();
    }

    void Update()
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(target.position);

        // ºäÆ÷Æ® ¡æ UI localPosition
        Vector2 minimapSize = minimapUI.rect.size;
        Vector2 uiPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapSize.x,
            (viewportPos.y - 0.5f) * minimapSize.y
        );

        // Clamp Ã³¸® (³×¸ð ¹Ù±ùÀ¸·Î ¾È ³ª°¡°Ô)
        float halfX = minimapSize.x / 2f;
        float halfY = minimapSize.y / 2f;

        uiPos.x = Mathf.Clamp(uiPos.x, -halfX, halfX);
        uiPos.y = Mathf.Clamp(uiPos.y, -halfY, halfY);

        markerUI.localPosition = uiPos;

        // ±ôºýÀÓ È¿°ú
        Color c = markerImage.color;
        c.a = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
        markerImage.color = c;
    }
}
