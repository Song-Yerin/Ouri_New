using UnityEngine;

public class AutoSpinner : MonoBehaviour
{
    [Tooltip("회전 축 (기본: 위 방향)")]
    public Vector3 axis = Vector3.up;

    [Tooltip("초당 회전 각도(도)")]
    public float degreesPerSecond = 90f;

    [Tooltip("월드 좌표계 기준으로 회전할지 여부 (기본: 로컬)")]
    public bool useWorldSpace = false;

    void Update()
    {
        if (axis.sqrMagnitude < 1e-6f) return; // 축이 0이면 무시
        var delta = axis.normalized * (degreesPerSecond * Time.deltaTime);
        transform.Rotate(delta, useWorldSpace ? Space.World : Space.Self);
    }
}
