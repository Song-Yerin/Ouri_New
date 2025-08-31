using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    public Transform target;         // 따라다닐 타겟
    public Vector3 offset = new Vector3(0f, 2f, 0f); // 머리 위 위치 조정

    void LateUpdate()
    {
        if (target == null) return;

        // 타겟 위치 + 오프셋으로 이동
        transform.position = target.position + offset;

        // 카메라 바라보도록 회전
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}
