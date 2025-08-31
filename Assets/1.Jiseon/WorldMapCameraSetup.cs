using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WorldMapCameraSetup : MonoBehaviour
{
    public Transform player;  // 플레이어 Transform
    private Camera cam;
    public GameObject worldMapUI;

    [Header("Zoom Settings")]
    public float zoomSpeed = 200f;
    public float minZoom = 50f;
    public float maxZoom = 800f;

    private Vector3 terrainMin;  // 맵 최소 좌표
    private Vector3 terrainMax;  // 맵 최대 좌표

    public bool isback = false;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 씬에 있는 모든 Terrain 기준으로 경계 계산
        CalculateTerrainBounds();

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        cam.orthographic = true;

        // 기본 줌 (70%)
        float mapSize = Mathf.Max(terrainMax.x - terrainMin.x, terrainMax.z - terrainMin.z);
        cam.orthographicSize = mapSize * 0.35f;

        // 최대 줌은 Terrain 전체가 딱 보이는 수준까지만
        maxZoom = mapSize * 0.5f;

        // Clear Flags 색 보정
        if (isback)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.3f, 0.1f); // 초록톤 (Terrain 색과 맞춤)
        }
        else
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.3f, 0.1f, 0f); // 초록톤 (Terrain 색과 맞춤)
        }
       
        
    }

    void CalculateTerrainBounds()
    {
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        if (terrains.Length == 0) return;

        // 첫 번째 Terrain으로 초기화
        Vector3 min = terrains[0].transform.position;
        Vector3 max = min + terrains[0].terrainData.size;

        // 모든 Terrain을 돌면서 최소/최대 갱신
        foreach (var t in terrains)
        {
            Vector3 pos = t.transform.position;
            Vector3 size = t.terrainData.size;

            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos + size);
        }

        terrainMin = min;
        terrainMax = max;

        Debug.Log($"전체 맵 경계: {terrainMin} ~ {terrainMax}");
    }

    void LateUpdate()
    {
        if (cam == null || player == null) return;

        // Tab으로 켜고 끄기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            worldMapUI.SetActive(!worldMapUI.activeSelf);
        }

        if (!worldMapUI.activeSelf) return;

        // 마우스 휠 줌인/줌아웃
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        // 플레이어 중심 위치
        Vector3 targetPos = player.position;

        // 카메라 높이는 Terrain 위 고정
        targetPos.y = terrainMax.y + 100f;

        // 카메라 절반 범위 계산
        float vertExtent = cam.orthographicSize;
        float horzExtent = cam.orthographicSize * cam.aspect;

        // Terrain 경계에 맞게 클램프
        float minX = terrainMin.x + horzExtent;
        float maxX = terrainMax.x - horzExtent;
        float minZ = terrainMin.z + vertExtent;
        float maxZ = terrainMax.z - vertExtent;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        // 최종 카메라 위치 반영
        transform.position = targetPos;
    }
}
