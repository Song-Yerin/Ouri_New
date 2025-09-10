using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LookUpZone : MonoBehaviour
{
    [Header("Canvas 쪽 LookUpCamUI (자동 탐색 가능)")]
    public LookUpCamUI ui;

    [Header("이 존에서 '위로 보기' 시 카메라가 설 위치/회전")]
    public Transform viewPoint;

    [Tooltip("viewPoint가 비어있으면 자식에서 자동으로 찾아옵니다. (이름 'ViewPoint' 우선)")]
    public bool autoFindChildViewPoint = true;

    void Reset()
    {
        // 존 콜라이더는 반드시 Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        // UI 자동 탐색
        if (ui == null) ui = FindObjectOfType<LookUpCamUI>(true);

        // viewPoint 자동 탐색
        if (autoFindChildViewPoint && viewPoint == null)
        {
            // 1) 이름이 'ViewPoint'인 자식 먼저
            var t = transform.Find("ViewPoint");
            if (t != null) viewPoint = t;
            // 2) 없으면 첫 번째 자식
            else if (transform.childCount > 0) viewPoint = transform.GetChild(0);
        }

        if (ui == null)
            Debug.LogWarning($"[LookUpZone] LookUpCamUI를 찾지 못했습니다. ({name})");
        if (viewPoint == null)
            Debug.LogWarning($"[LookUpZone] viewPoint가 비었습니다. ({name})");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // (멀티플레이 시 로컬 플레이어만 보이게 하려면 여기서 권한 체크)
        ui?.EnterZone(viewPoint);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ui?.ExitZone(viewPoint);
    }

#if UNITY_EDITOR
    // 에디터에서 존의 viewPoint를 보기 쉽게 그려줌
    void OnDrawGizmosSelected()
    {
        if (viewPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(viewPoint.position, 0.15f);
        Gizmos.DrawRay(viewPoint.position, viewPoint.forward * 0.8f);
    }
#endif
}
