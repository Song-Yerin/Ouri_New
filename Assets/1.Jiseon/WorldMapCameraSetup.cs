using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WorldMapCameraSetup : MonoBehaviour
{
    public Transform player;  // �÷��̾� Transform
    private Camera cam;
    public GameObject worldMapUI;

    [Header("Zoom Settings")]
    public float zoomSpeed = 200f;
    public float minZoom = 50f;
    public float maxZoom = 800f;

    private Vector3 terrainMin;  // �� �ּ� ��ǥ
    private Vector3 terrainMax;  // �� �ִ� ��ǥ

    public bool isback = false;

    void Start()
    {
        cam = GetComponent<Camera>();

        // ���� �ִ� ��� Terrain �������� ��� ���
        CalculateTerrainBounds();

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        cam.orthographic = true;

        // �⺻ �� (70%)
        float mapSize = Mathf.Max(terrainMax.x - terrainMin.x, terrainMax.z - terrainMin.z);
        cam.orthographicSize = mapSize * 0.35f;

        // �ִ� ���� Terrain ��ü�� �� ���̴� ���ر�����
        maxZoom = mapSize * 0.5f;

        // Clear Flags �� ����
        if (isback)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.3f, 0.1f); // �ʷ��� (Terrain ���� ����)
        }
        else
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.3f, 0.1f, 0f); // �ʷ��� (Terrain ���� ����)
        }
       
        
    }

    void CalculateTerrainBounds()
    {
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        if (terrains.Length == 0) return;

        // ù ��° Terrain���� �ʱ�ȭ
        Vector3 min = terrains[0].transform.position;
        Vector3 max = min + terrains[0].terrainData.size;

        // ��� Terrain�� ���鼭 �ּ�/�ִ� ����
        foreach (var t in terrains)
        {
            Vector3 pos = t.transform.position;
            Vector3 size = t.terrainData.size;

            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos + size);
        }

        terrainMin = min;
        terrainMax = max;

        Debug.Log($"��ü �� ���: {terrainMin} ~ {terrainMax}");
    }

    void LateUpdate()
    {
        if (cam == null || player == null) return;

        // Tab���� �Ѱ� ����
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            worldMapUI.SetActive(!worldMapUI.activeSelf);
        }

        if (!worldMapUI.activeSelf) return;

        // ���콺 �� ����/�ܾƿ�
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        // �÷��̾� �߽� ��ġ
        Vector3 targetPos = player.position;

        // ī�޶� ���̴� Terrain �� ����
        targetPos.y = terrainMax.y + 100f;

        // ī�޶� ���� ���� ���
        float vertExtent = cam.orthographicSize;
        float horzExtent = cam.orthographicSize * cam.aspect;

        // Terrain ��迡 �°� Ŭ����
        float minX = terrainMin.x + horzExtent;
        float maxX = terrainMax.x - horzExtent;
        float minZ = terrainMin.z + vertExtent;
        float maxZ = terrainMax.z - vertExtent;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        // ���� ī�޶� ��ġ �ݿ�
        transform.position = targetPos;
    }
}
