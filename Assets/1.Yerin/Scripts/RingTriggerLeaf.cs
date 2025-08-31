using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RingTriggerLeaf : MonoBehaviour
{
    public enum Kind { Outer, Inner }
    [Tooltip("이 트리거가 바깥(Outer)인지 안쪽(Inner)인지 지정")]
    public Kind kind;

    [Tooltip("링 루트에 붙은 GlideRingAccelerator를 드래그해서 연결")]
    public GlideRingAccelerator parent;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => parent?.NotifyEnter(kind, other);
    private void OnTriggerExit(Collider other) => parent?.NotifyExit(kind, other);
}
