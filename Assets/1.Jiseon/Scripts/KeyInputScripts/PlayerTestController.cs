using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTestController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    [Header("���콺 ȸ�� ����")]
    public float mouseSensitivity = 100f; // ���콺 ����
    private float xRotation = 0f; // ī�޶� Pitch(����) ���� ��

    public Transform playerCamera;  // ���� �ִ� ī�޶� ���� (�ν����Ϳ��� �巡��)
    public Vector3 cameraOffset = new Vector3(0, 1.6f, 0); // ī�޶� ��ġ (�Ӹ� ��ġ ����)

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // ���콺 Ŀ�� ����� ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // �̵�
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // �߷� ����
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ���콺 �Է� �ޱ�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �¿� ȸ��(Yaw) �� �÷��̾� ��ü ȸ��
        transform.Rotate(Vector3.up * mouseX);

        // ���� ȸ��(Pitch) �� ī�޶� ȸ��
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (playerCamera != null)
        {
            // Pitch�� ���� (Yaw�� ��ü�� ���������Ƿ� �ʿ� ����)
            playerCamera.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);

            // ī�޶� ��ġ ������Ʈ
            playerCamera.position = transform.position + cameraOffset;
        }
    }
}
