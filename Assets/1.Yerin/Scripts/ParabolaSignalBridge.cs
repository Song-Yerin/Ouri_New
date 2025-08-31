using UnityEngine;

public class ParabolaSignalBridge : MonoBehaviour
{
    public ParabolaMover mover;   // ���� �������� ParabolaMover
    public Transform target;      // ���� ����(Transform)
    public float flightTime = 1.2f;
    public float groundY = 0f;
    public float horizontalSpeed = 20f;

    // Ÿ����� ��Ȯ�� T�ʿ� ����
    public void LaunchToTarget()
    {
        if (mover == null) mover = GetComponent<ParabolaMover>();
        mover.LaunchTo(target ? target.position : transform.position, flightTime);
    }

    // ���� �ӵ��� �����ϰ� XZ�� �޷� ����
    public void LaunchToTargetXZ()
    {
        if (mover == null) mover = GetComponent<ParabolaMover>();
        Vector3 tgt = target ? target.position : transform.position;
        mover.LaunchToXZ(tgt, groundY, horizontalSpeed);
    }
}
