using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    public Transform target;         // ����ٴ� Ÿ��
    public Vector3 offset = new Vector3(0f, 2f, 0f); // �Ӹ� �� ��ġ ����

    void LateUpdate()
    {
        if (target == null) return;

        // Ÿ�� ��ġ + ���������� �̵�
        transform.position = target.position + offset;

        // ī�޶� �ٶ󺸵��� ȸ��
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}
