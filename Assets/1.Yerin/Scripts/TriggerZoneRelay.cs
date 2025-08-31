using UnityEngine;

public class TriggerZoneRelay : MonoBehaviour
{
    public enum ZoneKind { Start, End }
    [SerializeField] private ZoneKind kind = ZoneKind.Start;
    [SerializeField] private GimmickSegment target;  // �Ŵ��� ����
    [SerializeField] private Transform playerRoot;    // �÷��̾� ��Ʈ(�ʼ� �ƴ�)

    private void Reset()
    {
        // �ڵ����� ���� ������ �Ŵ��� ã�� ���� �õ�(����)
        if (!target) target = FindObjectOfType<GimmickSegment>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾ ���͸� �ϰ� ������ �Ʒ� ���� ����. �ʿ������ ����.
        if (playerRoot && !other.transform.IsChildOf(playerRoot)) return;

        if (!target) return;

        if (kind == ZoneKind.Start) target.OnStartEntered();
        else target.OnEndEntered();
    }
}
