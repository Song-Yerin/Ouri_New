using UnityEngine;
using UnityEngine.UI;

public class WorldMapMarker : MonoBehaviour
{
    public Transform target;           // ����/NPC
    public RectTransform markerUI;     // ��Ŀ ������
    public Camera worldMapCamera;      // ����� ī�޶�
    public RectTransform worldMapUI;   // RawImage RectTransform
    public RectTransform coneUI;   // �ﰢ�� �̹���
    void Update()
    {
        Vector3 viewportPos = worldMapCamera.WorldToViewportPoint(target.position);

        // ī�޶� �ڿ� ������ ����
        if (viewportPos.z < 0)
        {
            markerUI.gameObject.SetActive(false);
            return;
        }
        else
        {
            markerUI.gameObject.SetActive(true);
        }

        // ����Ʈ(0~1) �� UI ��ǥ(-width/2 ~ width/2)
        Vector2 mapSize = worldMapUI.rect.size;
        Vector2 uiPos = new Vector2(
            (viewportPos.x * mapSize.x) - (mapSize.x * 0.5f),
            (viewportPos.y * mapSize.y) - (mapSize.y * 0.5f)
        );

        markerUI.localPosition = uiPos;

        coneUI.localEulerAngles = new Vector3(0, 0, -target.eulerAngles.y);
        coneUI.localPosition = uiPos;
    }
}
