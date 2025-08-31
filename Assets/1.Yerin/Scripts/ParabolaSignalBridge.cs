using UnityEngine;

public class ParabolaSignalBridge : MonoBehaviour
{
    public ParabolaMover mover;   // 같은 프리팹의 ParabolaMover
    public Transform target;      // 착지 지점(Transform)
    public float flightTime = 1.2f;
    public float groundY = 0f;
    public float horizontalSpeed = 20f;

    // 타깃까지 정확히 T초에 도달
    public void LaunchToTarget()
    {
        if (mover == null) mover = GetComponent<ParabolaMover>();
        mover.LaunchTo(target ? target.position : transform.position, flightTime);
    }

    // 수평 속도를 고정하고 XZ로 달려 착지
    public void LaunchToTargetXZ()
    {
        if (mover == null) mover = GetComponent<ParabolaMover>();
        Vector3 tgt = target ? target.position : transform.position;
        mover.LaunchToXZ(tgt, groundY, horizontalSpeed);
    }
}
