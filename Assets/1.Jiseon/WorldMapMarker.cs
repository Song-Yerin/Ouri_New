using UnityEngine;
using UnityEngine.UI;

public class WorldMapMarker : MonoBehaviour
{
    public Transform target;           // 보물/NPC
    public RectTransform markerUI;     // 마커 아이콘
    public Camera worldMapCamera;      // 월드맵 카메라
    public RectTransform worldMapUI;   // RawImage RectTransform
    public RectTransform coneUI;   // 삼각형 이미지
    void Update()
    {
        Vector3 viewportPos = worldMapCamera.WorldToViewportPoint(target.position);

        // 카메라 뒤에 있으면 숨김
        if (viewportPos.z < 0)
        {
            markerUI.gameObject.SetActive(false);
            return;
        }
        else
        {
            markerUI.gameObject.SetActive(true);
        }

        // 뷰포트(0~1) → UI 좌표(-width/2 ~ width/2)
        Vector2 mapSize = worldMapUI.rect.size;
        Vector2 uiPos = new Vector2(
            (viewportPos.x * mapSize.x) - (mapSize.x * 0.5f),
            (viewportPos.y * mapSize.y) - (mapSize.y * 0.5f)
        );

        markerUI.localPosition = uiPos;

        coneUI.localEulerAngles = new Vector3(0, 0, -target.eulerAngles.y);
        coneUI.localPosition = uiPos;
    }
}
