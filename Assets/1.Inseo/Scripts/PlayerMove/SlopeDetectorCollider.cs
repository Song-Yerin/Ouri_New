using UnityEngine;

/// <summary>
/// Trigger Collider로 경사면을 감지하는 컴포넌트
/// 플레이어 자식 오브젝트로 생성
/// </summary>
public class SlopeDetectorCollider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float slopeLimit = 45f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // Public 프로퍼티
    public bool IsOnSteepSlope { get; private set; }
    public float CurrentSlopeAngle { get; private set; }
    public Vector3 SlideDirection { get; private set; }

    private SphereCollider detectorCollider;
    private Vector3 lastGroundNormal = Vector3.up;

    private void Awake()
    {
        // Sphere Collider 자동 추가/가져오기
        detectorCollider = GetComponent<SphereCollider>();
        if (detectorCollider == null)
        {
            detectorCollider = gameObject.AddComponent<SphereCollider>();
        }

        // Trigger 설정
        detectorCollider.isTrigger = true;
        detectorCollider.radius = 0.5f; // 필요에 따라 조정

        // Center를 약간 아래로 (발 위치)
        detectorCollider.center = Vector3.down * 0.3f;
    }

    private void OnTriggerStay(Collider other)
    {
        // Ground Layer 체크
        if (((1 << other.gameObject.layer) & groundLayer) == 0)
            return;

        // 충돌 지점에서 가장 가까운 지점 찾기
        Vector3 closestPoint = other.ClosestPoint(transform.position);

        // 해당 지점의 법선 벡터 구하기 (Raycast 사용)
        Vector3 checkOrigin = closestPoint + Vector3.up * 0.5f;

        if (Physics.Raycast(checkOrigin, Vector3.down, out RaycastHit hit, 1f, groundLayer))
        {
            lastGroundNormal = hit.normal;
            CurrentSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (CurrentSlopeAngle > slopeLimit)
            {
                IsOnSteepSlope = true;
                SlideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

                if (showDebug)
                {
                    // 충돌 지점 표시 (빨간 구체)
                    Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.5f, Color.red);

                    // 법선 벡터 (파란 선)
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue);

                    // 미끄러짐 방향 (노란 화살표)
                    Debug.DrawRay(transform.parent.position, SlideDirection * 2f, Color.yellow);
                }
            }
            else
            {
                IsOnSteepSlope = false;
                SlideDirection = Vector3.zero;

                if (showDebug)
                {
                    // 일반 지면 (초록 선)
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Ground Layer 체크
        if (((1 << other.gameObject.layer) & groundLayer) == 0)
            return;

        // 지면을 벗어나면 초기화
        IsOnSteepSlope = false;
        CurrentSlopeAngle = 0f;
        SlideDirection = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (detectorCollider == null) return;

        // Trigger 범위 시각화 (와이어프레임 구체)
        Gizmos.color = IsOnSteepSlope ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position + detectorCollider.center, detectorCollider.radius);
    }
}
