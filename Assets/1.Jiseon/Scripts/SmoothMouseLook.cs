using UnityEngine;

public class SmoothMouseLook : MonoBehaviour
{
    [Header("Mouse Look 설정")]
    public float sensitivity = 100f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("ForwardMove 설정")]
    public float forwardSpeed = 5f;
    public float heightOffset = 1.8f;

    [Header("제어 플래그")]
    [Tooltip("마우스로 카메라 회전 허용 여부 (Inspector에서 수동 조정 가능)")]
    public bool allowMouseLook = true;   // 인스펙터에서 직접 켜고 끄기 가능

    private float pitch;
    private float yaw;
    private Terrain terrain;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        terrain = Terrain.activeTerrain;

        Vector3 e = transform.localEulerAngles;
        pitch = e.x > 180f ? e.x - 360f : e.x;
        yaw = e.y > 180f ? e.y - 360f : e.y;
    }

    void Update()
    {
        // allowMouseLook이 true일 때만 마우스 회전 허용
        if (allowMouseLook)
        {
            float mx = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float my = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

            yaw += mx;
            pitch -= my;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // ForwardMove & 지형 높이 보정
        Vector3 flatForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 newPos = cam.position + flatForward * forwardSpeed * Time.deltaTime;

        float terrainY = terrain.SampleHeight(newPos);
        newPos.y = terrainY + heightOffset;
        cam.position = newPos;
    }
}
