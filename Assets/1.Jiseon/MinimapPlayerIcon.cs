using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public RectTransform playerIcon;  // 화살표 아이콘
    public Transform player;          // 플레이어 Transform

    void Update()
    {
        // 플레이어 Y축 회전값만 아이콘에 적용
        float yRotation = player.eulerAngles.y;
        playerIcon.localEulerAngles = new Vector3(0, 0, -yRotation);
    }
}
