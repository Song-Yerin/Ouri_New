using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System;

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

    [Header("Terrain 설정")]
    public Terrain targetTerrain;   // 직접 넣을 Terrain (없으면 씬 전체 Terrain 기준)

    void Start()
    {
        cam = GetComponent<Camera>();
        SetupWorldMapCamera_DisableAllVolumes();
        // Terrain 기준으로 경계 계산
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
            cam.backgroundColor = new Color(0.1f, 0.3f, 0.1f, 0f); // 초록톤 + 알파 0
        }
    }

    void SetupWorldMapCamera_DisableAllVolumes()
    {
        var ucam = GetComponent<UniversalAdditionalCameraData>();
        if (!ucam) ucam = gameObject.AddComponent<UniversalAdditionalCameraData>();
        ucam.renderPostProcessing = false;
        ucam.volumeLayerMask = 0; // Nothing
    }

    void CalculateTerrainBounds()
    {
        // 직접 Terrain이 지정돼 있다면 그거만 기준
        if (targetTerrain != null)
        {
            Vector3 pos = targetTerrain.transform.position;
            Vector3 size = targetTerrain.terrainData.size;

            terrainMin = pos;
            terrainMax = pos + size;

            Debug.Log($"선택된 Terrain 기준 경계: {terrainMin} ~ {terrainMax}");
            return;
        }

        // 지정 안 했으면 씬 전체 Terrain 합산
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        if (terrains.Length == 0) return;

        Vector3 min = terrains[0].transform.position;
        Vector3 max = min + terrains[0].terrainData.size;

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
