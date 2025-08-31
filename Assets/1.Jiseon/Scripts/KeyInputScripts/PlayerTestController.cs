using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTestController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("마우스 회전 설정")]
    public float mouseSensitivity = 100f; // 마우스 감도
    private float xRotation = 0f; // 카메라 Pitch(상하) 누적 값

    public Transform playerCamera;  // 씬에 있는 카메라 참조 (인스펙터에서 드래그)
    public Vector3 cameraOffset = new Vector3(0, 1.6f, 0); // 카메라 위치 (머리 위치 정도)

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 마우스 커서 숨기고 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 이동
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 좌우 회전(Yaw) → 플레이어 본체 회전
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전(Pitch) → 카메라만 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (playerCamera != null)
        {
            // Pitch만 적용 (Yaw는 본체에 적용했으므로 필요 없음)
            playerCamera.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);

            // 카메라 위치 업데이트
            playerCamera.position = transform.position + cameraOffset;
        }
    }
}
