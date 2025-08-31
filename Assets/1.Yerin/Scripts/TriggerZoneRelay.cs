using UnityEngine;

public class TriggerZoneRelay : MonoBehaviour
{
    public enum ZoneKind { Start, End }
    [SerializeField] private ZoneKind kind = ZoneKind.Start;
    [SerializeField] private GimmickSegment target;  // 매니저 참조
    [SerializeField] private Transform playerRoot;    // 플레이어 루트(필수 아님)

    private void Reset()
    {
        // 자동으로 같은 씬에서 매니저 찾아 연결 시도(선택)
        if (!target) target = FindObjectOfType<GimmickSegment>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 필터링 하고 싶으면 아래 조건 유지. 필요없으면 제거.
        if (playerRoot && !other.transform.IsChildOf(playerRoot)) return;

        if (!target) return;

        if (kind == ZoneKind.Start) target.OnStartEntered();
        else target.OnEndEntered();
    }
}
