using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(CharacterController), typeof(Animator))]
public class NavMeshAI : MonoBehaviour
{
    private enum ChaseMode { NavMesh, CharacterController }
    private ChaseMode currentMode = ChaseMode.NavMesh;

    [Header("추격 대상")]
    [SerializeField] private Transform player;

    [Header("컴포넌트 참조")]
    private NavMeshAgent agent;
    private CharacterController controller;
    private Animator anim;

    [Header("경로 확인 및 전환 설정")]
    [SerializeField] private float pathCheckInterval = 0.5f;
    private float pathCheckTimer;

    [Header("스마트 점프 설정")]
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float jumpHeightMargin = 0.5f;
    private bool isJumping = false;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity;

    // --- [핵심 추가 1] ---
    [Header("점프 쿨다운")]
    [SerializeField] private float jumpCooldown = 3.0f; // 점프 또는 모드 전환 후 대기 시간
    private float lastActionTime = -10f; // 마지막 행동(점프/모드전환) 시간

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
            if (agent.enabled) agent.ResetPath();
            anim.SetBool(runningBool, false);
            return;
        }

        pathCheckTimer -= Time.deltaTime;
        if (pathCheckTimer <= 0f)
        {
            pathCheckTimer = pathCheckInterval;
            if (!isJumping) CheckNavMeshPath();
        }

        if (currentMode == ChaseMode.NavMesh)
        {
            UpdateNavMeshMode();
        }
        else
        {
            UpdateCharacterControllerMode();
        }
    }

    private void UpdateNavMeshMode()
    {
        if (agent.enabled)
        {
            agent.SetDestination(player.position);
        }
        anim.SetBool(runningBool, agent.velocity.magnitude > 0.1f);
    }

    private void UpdateCharacterControllerMode()
    {
        if (isJumping) return;

        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);

        if (controller.isGrounded)
        {
            // 점프를 시도하고, 실패하면 플레이어 방향으로 이동
            if (!AnalyzeAndTrySmartJump())
            {
                // 플레이어를 향해 이동 (대체 행동)
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
        }
    }

    // --- [핵심 수정 2] ---
    private void CheckNavMeshPath()
    {
        if (player == null || !this.enabled) return;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = NavMesh.CalculatePath(transform.position, player.position, agent.areaMask, path);

        if (!hasPath || path.status != NavMeshPathStatus.PathComplete)
        {
            // 쿨다운이 지났을 때만 Controller 모드로 전환 시도
            if (currentMode == ChaseMode.NavMesh && Time.time >= lastActionTime + jumpCooldown)
            {
                lastActionTime = Time.time; // 행동 시간 기록
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

    private bool AnalyzeAndTrySmartJump()
    {
        // 쿨다운 중이면 점프 시도 안 함
        if (Time.time < lastActionTime + jumpCooldown) return false;

        // 조건 2: 플레이어가 내 전방에 있는가? (등 뒤에 있다면 점프 무의미)
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, directionToPlayer) < 0.3f) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, 2f))
        {
            lastActionTime = Time.time; // 점프 시간 기록
            StartCoroutine(ExecuteSmartJump(hit));
            return true;
        }
        return false;
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
            Vector3 verticalMove = verticalVelocity * Time.deltaTime;
            controller.Move(forwardMove + verticalMove);

            if (controller.isGrounded && verticalVelocity.y < 0) break;

            yield return null;
        }

        isJumping = false;
        // 점프가 끝난 직후, 바로 NavMesh 복귀를 시도
        CheckNavMeshPath();
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
