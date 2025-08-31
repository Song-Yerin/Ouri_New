using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // 플레이어 Transform

    void LateUpdate()
    {
        // 위치 따라가기 (X,Z만)
        Vector3 newPos = player.position;
        newPos.y = transform.position.y; // 높이는 고정
        transform.position = newPos;

        // 플레이어 방향 따라가기 (Y축 회전만)
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
