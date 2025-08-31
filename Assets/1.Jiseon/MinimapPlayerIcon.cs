using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public RectTransform playerIcon;  // ȭ��ǥ ������
    public Transform player;          // �÷��̾� Transform

    void Update()
    {
        // �÷��̾� Y�� ȸ������ �����ܿ� ����
        float yRotation = player.eulerAngles.y;
        playerIcon.localEulerAngles = new Vector3(0, 0, -yRotation);
    }
}
