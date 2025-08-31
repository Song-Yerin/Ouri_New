using UnityEngine;

public class SmoothMouseLook : MonoBehaviour
{
    [Header("Mouse Look 설정")]
    [Tooltip("마우스 감도")]
    public float sensitivity = 100f;
    [Tooltip("상하 회전 제한 (°)")]
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("ForwardMove 설정")]
    [Tooltip("전진 속도 (유닛/초)")]
    public float forwardSpeed = 5f;
    [Tooltip("지형 위 카메라 높이 오프셋")]
    public float heightOffset = 1.8f;

    private float pitch;
    private float yaw;
    private Terrain terrain;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        terrain = Terrain.activeTerrain;

        // 현재 회전값을 pitch, yaw로 분리
        Vector3 e = transform.localEulerAngles;
        pitch = e.x > 180f ? e.x - 360f : e.x;
        yaw = e.y > 180f ? e.y - 360f : e.y;
    }

    void Update()
    {
        // 1) Mouse Look 회전
        float mx = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 2) ForwardMove & 지형 높이 보정
        Vector3 flatForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 newPos = cam.position + flatForward * forwardSpeed * Time.deltaTime;

        float terrainY = terrain.SampleHeight(newPos);
        newPos.y = terrainY + heightOffset;
        cam.position = newPos;
    }
}
