using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    /// <summary>
    /// 추격자가 플레이어와 너무 멀어지거나 특정 레이어에 닿으면 
    /// 플레이어 근처 NavMesh로 텔레포트하는 안전장치
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ChaserTeleportSafety : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform player;

        [Header("Distance Check")]
        [SerializeField] private float maxChaseDistance = 50f;
        [SerializeField] private float checkInterval = 1f;

        [Header("Teleport Settings")]
        [SerializeField] private float teleportMinDistance = 5f;
        [SerializeField] private float teleportMaxDistance = 10f;
        [SerializeField] private int teleportAttempts = 10;

        [Header("Danger Layers")]
        [SerializeField] private LayerMask dangerLayers; // Water, Trap 등

        private NavMeshAgent agent;
        private float lastCheckTime;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            // 플레이어 자동 찾기
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // 일정 주기로 거리 체크
            if (Time.time - lastCheckTime > checkInterval)
            {
                lastCheckTime = Time.time;
                CheckDistanceAndTeleport();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Danger Layer에 닿으면 즉시 텔레포트
            if (((1 << other.gameObject.layer) & dangerLayers) != 0)
            {
                Debug.Log($"[ChaserTeleport] Danger Layer 감지: {LayerMask.LayerToName(other.gameObject.layer)}");
                TeleportNearPlayer();
            }
        }

        private void CheckDistanceAndTeleport()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > maxChaseDistance)
            {
                Debug.Log($"[ChaserTeleport] 너무 멀어짐 ({distance:F1}m > {maxChaseDistance}m)");
                TeleportNearPlayer();
            }
        }

        private void TeleportNearPlayer()
        {
            Vector3 teleportPosition;

            if (FindValidTeleportPosition(out teleportPosition))
            {
                // NavMeshAgent 일시 정지
                agent.enabled = false;

                // 텔레포트
                transform.position = teleportPosition;

                // NavMeshAgent 재활성화
                agent.enabled = true;

                Debug.Log($"[ChaserTeleport] 텔레포트 완료: {teleportPosition}");
            }
            else
            {
                Debug.LogWarning("[ChaserTeleport] 유효한 텔레포트 위치를 찾지 못했습니다!");
            }
        }

        private bool FindValidTeleportPosition(out Vector3 result)
        {
            result = Vector3.zero;

            for (int i = 0; i < teleportAttempts; i++)
            {
                // 플레이어 주변 랜덤 위치 생성
                Vector2 randomCircle = Random.insideUnitCircle;
                float randomDistance = Random.Range(teleportMinDistance, teleportMaxDistance);

                Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y).normalized;
                Vector3 randomPoint = player.position + randomDirection * randomDistance;

                // NavMesh 위의 가장 가까운 지점 찾기
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    // 플레이어까지의 경로가 존재하는지 확인
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(hit.position, player.position, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            result = hit.position;

                            // 디버그 시각화
                            Debug.DrawLine(result, result + Vector3.up * 3f, Color.cyan, 2f);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // Gizmo로 최대 추적 거리 시각화
        private void OnDrawGizmosSelected()
        {
            if (player == null) return;

            // 최대 추적 거리 (빨간 원)
            Gizmos.color = Color.red;
            DrawCircle(player.position, maxChaseDistance, 32);

            // 텔레포트 범위 (초록 원)
            Gizmos.color = Color.green;
            DrawCircle(player.position, teleportMinDistance, 16);
            DrawCircle(player.position, teleportMaxDistance, 16);
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
