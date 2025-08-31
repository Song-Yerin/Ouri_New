using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera cam; // Main Camera

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            // 카메라가 바라보는 방향 기준으로 이동 방향 회전
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cam.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 moveDir = camForward * v + camRight * h;
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;

            // (선택) 이동 방향을 바라보게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 2f);
        }
    }
}
