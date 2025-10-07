using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapMarkerManager : MonoBehaviour
{
    [System.Serializable]
    public class MarkerTarget
    {
        public Transform target;              // 추적할 오브젝트
        [HideInInspector] public RectTransform markerUI;  // 개별 마커 UI (자동 생성)
    }

    [Header("필수 참조")]
    public Camera minimapCamera;              // 미니맵용 카메라
    public RectTransform minimapUI;           // 미니맵 RawImage의 RectTransform
    public GameObject markerPrefab;           // 프리팹 (Image 오브젝트)

    [Header("마커 대상 리스트")]
    public List<MarkerTarget> markers = new List<MarkerTarget>();

    void Start()
    {
        // 각 타겟마다 마커 생성
        foreach (var marker in markers)
        {
            if (marker.target == null) continue;

            // 프리팹으로 마커 UI 생성
            GameObject newMarker = Instantiate(markerPrefab, minimapUI);
            marker.markerUI = newMarker.GetComponent<RectTransform>();

            // 이름 구분하기 쉽게 설정
            newMarker.name = "Marker_" + marker.target.name;
        }
    }

    void Update()
    {
        foreach (var marker in markers)
        {
            if (marker.target == null || marker.markerUI == null) continue;

            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(marker.target.position);
            Image img = marker.markerUI.GetComponent<Image>();

            // 미니맵 안에 있는지 판정
            bool inside = viewportPos.z > 0 &&
                          viewportPos.x > 0 && viewportPos.x < 1 &&
                          viewportPos.y > 0 && viewportPos.y < 1;

            if (inside)
            {
                // 뷰포트 좌표 → 미니맵 UI 좌표
                Vector2 minimapSize = minimapUI.rect.size;
                Vector2 uiPos = new Vector2(
                    (viewportPos.x - 0.5f) * minimapSize.x,
                    (viewportPos.y - 0.5f) * minimapSize.y
                );

                marker.markerUI.localPosition = uiPos;
                img.enabled = true;
            }
            else
            {
                img.enabled = false;
            }
        }
    }
}
