using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RingTriggerLeaf : MonoBehaviour
{
    public enum Kind { Outer, Inner }
    [Tooltip("�� Ʈ���Ű� �ٱ�(Outer)���� ����(Inner)���� ����")]
    public Kind kind;

    [Tooltip("�� ��Ʈ�� ���� GlideRingAccelerator�� �巡���ؼ� ����")]
    public GlideRingAccelerator parent;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => parent?.NotifyEnter(kind, other);
    private void OnTriggerExit(Collider other) => parent?.NotifyExit(kind, other);
}
