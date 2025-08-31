using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // �÷��̾� Transform

    void LateUpdate()
    {
        // ��ġ ���󰡱� (X,Z��)
        Vector3 newPos = player.position;
        newPos.y = transform.position.y; // ���̴� ����
        transform.position = newPos;

        // �÷��̾� ���� ���󰡱� (Y�� ȸ����)
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
