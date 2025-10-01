using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CharacterController), typeof(Animator))]
public class NavMeshAI : MonoBehaviour
{
    // ... (기존 변수 선언은 그대로 유지) ...
    private enum ChaseMode { NavMesh, CharacterController }
    private ChaseMode currentMode = ChaseMode.NavMesh;

    [Header("추격 대상")]
    [SerializeField] private Transform player;

    [Header("컴포넌트 참조")]
    private NavMeshAgent agent;
    private CharacterController controller;
    private Animator anim;

    [Header("추격 속도 조절")]
    [Tooltip("플레이어와 가까울 때의 기본 속도")]
    [SerializeField] private float minSpeed = 5f;
    [Tooltip("플레이어와 멀 때의 최대 속도")]
    [SerializeField] private float maxSpeed = 10f;
    [Tooltip("최대 속도에 도달하게 되는 기준 거리")]
    [SerializeField] private float maxSpeedDistance = 30f;

    [Header("경로 확인 주기")]
    [SerializeField] private float pathCheckInterval = 0.5f;
    private float pathCheckTimer;

    [Header("스마트 점프 설정")]
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float jumpHeightMargin = 0.5f;

    [Header("절벽 감지 설정")]
    [Tooltip("이 높이보다 높은 절벽 앞에서는 이동을 멈춥니다.")]
    [SerializeField] private float maxFallHeight = 2.0f;
    [Tooltip("전방 몇 미터 앞의 지형을 확인할지 결정합니다.")]
    [SerializeField] private float cliffCheckDistance = 1.0f;

    private bool isJumping = false;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity;

    [Header("행동 쿨다운")]
    [Tooltip("점프나 모드 전환 등 다음 행동까지의 대기 시간")]
    [SerializeField] private float actionCooldown = 1.5f;
    private float lastActionTime = -10f;

    [Header("애니메이션 파라미터")]
    [SerializeField] private string runningBool = "isRunning";
    [SerializeField] private string jumpTrigger = "Jump";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        SwitchToNavMeshMode();
    }

    void Update()
    {
        if (player == null)
        {
            agent.speed = minSpeed;
            anim.speed = 1f;
            return;
        }

        // --- [핵심 추가 2] --- 매 프레임 속도 조절 함수 호출
        AdjustSpeedBasedOnDistance();

        if (currentMode == ChaseMode.NavMesh) UpdateNavMeshMode();
        else UpdateCharacterControllerMode();
    }

    private void UpdateNavMeshMode()
    {
        if (agent.enabled) agent.SetDestination(player.position);
        anim.SetBool(runningBool, agent.velocity.magnitude > 0.1f);

        pathCheckTimer -= Time.deltaTime;
        if (pathCheckTimer <= 0f)
        {
            pathCheckTimer = pathCheckInterval;
            CheckNavMeshPath();
        }
    }

    // --- [수정된 부분 1] --- 중복 제거 및 로직 통합
    private void CheckNavMeshPath()
    {
        if (player == null || !this.enabled) return;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = NavMesh.CalculatePath(transform.position, player.position, agent.areaMask, path);

        // --- [핵심 수정] ---
        // 경로가 완전하지 않더라도, 에이전트가 현재 Off-Mesh Link를 건너는 중이라면 모드를 전환하지 않습니다.
        if ((!hasPath || path.status != NavMeshPathStatus.PathComplete) && !agent.isOnOffMeshLink)
        {
            if (currentMode == ChaseMode.NavMesh && Time.time >= lastActionTime + actionCooldown)
            {
                lastActionTime = Time.time;
                SwitchToControllerMode();
            }
        }
        else
        {
            if (currentMode == ChaseMode.CharacterController)
            {
                SwitchToNavMeshMode();
            }
        }
    }

    // --- [핵심 수정 1] --- UpdateCharacterControllerMode 로직 변경
    private void UpdateCharacterControllerMode()
    {
        if (isJumping) return;

        CheckNavMeshPath();
        if (currentMode == ChaseMode.NavMesh) return;

        if (controller.isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);

        if (controller.isGrounded)
        {
            if (IsCliffAhead())
            {
                // 절벽을 만났을 때의 행동 결정
                MoveAtCliffEdge();
            }
            else
            {
                // 일반적인 상황: 점프를 시도하거나 플레이어에게 이동
                if (!AnalyzeAndTrySmartJump())
                {
                    MoveTowardsPlayer();
                }
            }
        }
    }

    // --- [핵심 추가 2] --- 절벽 가장자리에서 행동을 결정하는 새 함수
    private void MoveAtCliffEdge()
    {
        // 조건 1: 플레이어가 뛰어내릴 수 있는 위치에 있는가?
        bool isPlayerBelow = (transform.position.y - player.position.y) > maxFallHeight;

        // 조건 2: 플레이어가 땅에 붙어 있는가?
        CharacterController playerController = player.GetComponent<CharacterController>();
        bool isPlayerGrounded = playerController != null ? playerController.isGrounded : Physics.Raycast(player.position, Vector3.down, 0.5f);

        if (isPlayerBelow && isPlayerGrounded && Time.time >= lastActionTime + actionCooldown)
        {
            // 두 조건을 모두 만족하면, 플레이어를 향해 과감히 뛰어내림
            lastActionTime = Time.time;
            controller.Move(transform.forward * agent.speed * Time.deltaTime * 2f); // 앞으로 강하게 밀어서 떨어뜨림
            anim.SetBool(runningBool, true);
        }
        else
        {
            // 뛰어내릴 상황이 아니면, 플레이어에게 가장 가까운 절벽 가장자리 위치를 찾아 이동

            // 1. 플레이어의 위치를 AI와 같은 높이의 평면에 투영
            Vector3 targetPositionOnPlane = new Vector3(player.position.x, transform.position.y, player.position.z);

            // 2. 그 투영된 위치에서 가장 가까운, 이동 가능한 NavMesh 위의 점을 찾음 (탐색 반경은 충분히 크게 설정)
            if (NavMesh.SamplePosition(targetPositionOnPlane, out NavMeshHit hit, 100f, agent.areaMask))
            {
                // 3. 찾은 '가장 가까운 유효 지점(hit.position)'을 향해 이동
                Vector3 directionToTarget = (hit.position - transform.position);
                directionToTarget.y = 0;

                if (directionToTarget.magnitude > agent.stoppingDistance)
                {
                    controller.Move(directionToTarget.normalized * agent.speed * Time.deltaTime);
                    transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                    anim.SetBool(runningBool, true);
                }
                else
                {
                    anim.SetBool(runningBool, false);
                }
            }
            else
            {
                // 근처에서 유효한 지점을 찾지 못했다면 멈춤
                anim.SetBool(runningBool, false);
            }
        }
    }

    private bool IsCliffAhead()
    {
        Vector3 rayStart = transform.position + Vector3.up * (controller.height / 2);
        Vector3 targetPos = rayStart + transform.forward * cliffCheckDistance;

        RaycastHit hit;
        if (Physics.Raycast(targetPos, Vector3.down, out hit, maxFallHeight + controller.height))
        {
            if (transform.position.y - hit.point.y > maxFallHeight)
            {
                Debug.DrawLine(targetPos, hit.point, Color.red);
                return true;
            }
        }
        else
        {
            Debug.DrawRay(targetPos, Vector3.down * (maxFallHeight + controller.height), Color.red);
            return true;
        }

        Debug.DrawRay(targetPos, Vector3.down * (maxFallHeight + controller.height), Color.green);
        return false;
    }

    // --- [수정된 부분 2] --- 중복 제거 및 로직 통합
    private bool AnalyzeAndTrySmartJump()
    {
        if (Time.time < lastActionTime + actionCooldown) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < 0.3f) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, 2f))
        {
            lastActionTime = Time.time;
            StartCoroutine(ExecuteSmartJump(hit));
            return true;
        }
        return false;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 moveDirection = (player.position - transform.position);
        moveDirection.y = 0;

        if (moveDirection.magnitude > agent.stoppingDistance)
        {
            controller.Move(moveDirection.normalized * agent.speed * Time.deltaTime);
            if (moveDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDirection.normalized);
            anim.SetBool(runningBool, true);
        }
        else
        {
            anim.SetBool(runningBool, false);
        }
    }

    // --- [핵심 추가 3] --- 거리에 비례하여 속도를 조절하는 새 함수
    private void AdjustSpeedBasedOnDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // 거리를 0과 1 사이의 값으로 정규화 (0 = 최소 거리, 1 = 최대 거리)
        float speedRatio = Mathf.Clamp01(distance / maxSpeedDistance);

        // Lerp 함수를 이용해 minSpeed와 maxSpeed 사이의 현재 속도를 계산
        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, speedRatio);

        // (선택 사항) 애니메이션 재생 속도도 이동 속도에 맞춰 조절
        // 기본 속도(minSpeed)일 때 anim.speed = 1, 최대 속도(maxSpeed)일 때 anim.speed = 2가 되도록 설정 (비율은 조절 가능)
        anim.speed = Mathf.Lerp(1f, maxSpeed / minSpeed, speedRatio);
    }

    private IEnumerator ExecuteSmartJump(RaycastHit obstacleHit)
    {
        isJumping = true;
        anim.SetTrigger(jumpTrigger);

        float heightToClear = obstacleHit.collider.bounds.max.y - transform.position.y + jumpHeightMargin;
        heightToClear = Mathf.Max(heightToClear, jumpHeightMargin);
        heightToClear = Mathf.Min(heightToClear, maxJumpHeight);

        float requiredJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * heightToClear);
        verticalVelocity.y = requiredJumpVelocity;

        float jumpStartTime = Time.time;
        while (Time.time < jumpStartTime + 2.0f)
        {
            Vector3 forwardMove = transform.forward * agent.speed * Time.deltaTime;
            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(forwardMove + verticalVelocity * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity.y < 0) break;
            yield return null;
        }

        isJumping = false;
    }

    private void SwitchToNavMeshMode()
    {
        if (currentMode == ChaseMode.NavMesh && agent.enabled) return;
        currentMode = ChaseMode.NavMesh;
        controller.enabled = false;
        agent.enabled = true;
        agent.Warp(transform.position);
    }

    private void SwitchToControllerMode()
    {
        if (currentMode == ChaseMode.CharacterController) return;
        currentMode = ChaseMode.CharacterController;
        agent.enabled = false;
        controller.enabled = true;
        verticalVelocity = Vector3.zero;
        anim.SetBool(runningBool, false);
    }
}
