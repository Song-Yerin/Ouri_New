using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterController))]
public class SmartChaseAI : MonoBehaviour
{
    [Header("추격 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("점프 설정")]
    [Tooltip("최대 점프 높이에 도달하기 위한 기본 점프 힘")]
    [SerializeField] private float jumpForce = 8f;
    [Tooltip("AI가 점프할 수 있는 최대 높이")]
    [SerializeField] private float maxJumpHeight = 3f;
    [Tooltip("가장 낮은 장애물을 넘기 위한 최소 점프 힘")]
    [SerializeField] private float minObstacleJumpForce = 5f;
    [SerializeField] public float jumpCooldown = 2.0f;
    [SerializeField] private LayerMask structureLayerMask = -1;

    [Header("구조물 감지")]
    [SerializeField] private float groundCheckDistance = 1f;

    [Header("장애물 점프 설정")]
    [SerializeField] private float obstacleDetectDistance = 1.5f;
    [SerializeField] private float obstacleDetectHeight = 0.5f;
    [SerializeField] private LayerMask obstacleLayerMask = ~0;

    [Header("애니메이션 파라미터")]
    [SerializeField] private string runningBool = "isRunning";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string isAirborneBool = "isAirborne";
    [SerializeField] private string isFallingBool = "isFalling";
    [SerializeField] private string landTrigger = "Land";
    [SerializeField] private string verticalSpeedFloat = "VerticalSpeed";

    // 컴포넌트 참조
    private CharacterController controller;
    private Animator anim;

    // 상태 변수
    private bool isJumping = false;
    private bool wasGroundedLastFrame = true;
    private Vector3 verticalVelocity;
    private float gravity = -9.81f;
    private float lastJumpTime = -99f;

    // 구조물 정보
    private Transform currentTargetStructure;
    private Vector3 jumpTargetPoint;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        bool isGrounded = CheckGroundStatus();
        HandleJumpAnimations(isGrounded);
        HandleGravity();

        if (isJumping) return;

        if (isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            // 1. 전방 장애물 점프 판단
            if (DetectObstacle(out RaycastHit hit))
            {
                isJumping = true;
                lastJumpTime = Time.time;
                StartCoroutine(ExecuteJumpOverObstacle(hit));
                return;
            }

            // 2. 높은 곳의 플레이어를 향한 스마트 점프 판단
            if (AnalyzeAndTrySmartJump())
            {
                lastJumpTime = Time.time;
                return;
            }
        }

        MoveTowardsTarget();
        wasGroundedLastFrame = isGrounded;
    }

    /// <summary>
    /// 전방 장애물을 감지하고, 감지했다면 충돌 정보를 반환합니다.
    /// </summary>
    private bool DetectObstacle(out RaycastHit hit)
    {
        Vector3 origin = transform.position + (Vector3.up * obstacleDetectHeight);
        return Physics.Raycast(origin, transform.forward, out hit, obstacleDetectDistance, obstacleLayerMask);
    }

    /// <summary>
    /// 감지된 장애물의 높이에 맞춰 점프를 실행합니다.
    /// </summary>
    private IEnumerator ExecuteJumpOverObstacle(RaycastHit obstacleHit)
    {
        anim?.SetTrigger(jumpTrigger);

        // --- [핵심 수정: 물리 공식 기반 점프 힘 계산] ---
        // 1. 넘어야 할 실제 높이 계산 (장애물 상단 - 내 발 위치 + 약간의 여유)
        float obstacleTopY = obstacleHit.collider.bounds.max.y;
        float heightToClear = obstacleTopY - transform.position.y + 0.5f; // 0.5m 여유분 추가

        // 2. 점프 높이가 음수이거나 너무 높지 않도록 제한
        if (heightToClear < 0) heightToClear = 0.5f; // 최소 점프 높이
        heightToClear = Mathf.Min(heightToClear, maxJumpHeight);

        // 3. 필요한 초기 점프 속도 계산: v = sqrt(2 * g * h)
        //    g는 중력(gravity)의 절대값, h는 목표 높이(heightToClear)
        float requiredJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * heightToClear);

        // 4. 계산된 힘으로 점프
        verticalVelocity.y = requiredJumpVelocity;

        float jumpStartTime = Time.time;
        // 점프 시간 동안 앞으로 이동하며 포물선 운동
        while (!CheckGroundStatus() && Time.time < jumpStartTime + 2.0f) // 최대 2초간 점프 시도
        {
            // 점프 중에도 계속 앞으로 이동
            Vector3 forwardMove = transform.forward * moveSpeed * Time.deltaTime;

            // 수직 이동 (중력 적용)
            verticalVelocity.y += gravity * Time.deltaTime;
            Vector3 verticalMove = verticalVelocity * Time.deltaTime;

            // 수평, 수직 이동을 합쳐서 한 번에 Move 호출
            controller.Move(forwardMove + verticalMove);

            yield return null;
        }

        isJumping = false;
    }

    /// <summary>
    /// 점프 관련 애니메이션 상태를 관리합니다.
    /// </summary>
    private void HandleJumpAnimations(bool isGrounded)
    {
        if (anim == null) return;

        anim.SetBool(isAirborneBool, !isGrounded);
        anim.SetFloat(verticalSpeedFloat, verticalVelocity.y);
        anim.SetBool(isFallingBool, !isGrounded && verticalVelocity.y < -1f);

        if (!wasGroundedLastFrame && isGrounded)
        {
            anim.SetTrigger(landTrigger);
        }
    }

    /// <summary>
    /// 플레이어의 위치를 분석하고 스마트 점프를 시도합니다.
    /// </summary>
    private bool AnalyzeAndTrySmartJump()
    {
        if (player == null) return false;

        float heightDifference = player.position.y - transform.position.y;
        if (heightDifference <= 0.3f || heightDifference > (maxJumpHeight * 1.5f)) return false;

        Transform playerStructure = DetectPlayerStructure();
        if (playerStructure != null && playerStructure != currentTargetStructure)
        {
            if (CanJumpToStructure(playerStructure))
            {
                isJumping = true;
                currentTargetStructure = playerStructure;
                StartCoroutine(ExecuteSmartJump());
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 플레이어가 현재 서있는 구조물을 감지합니다.
    /// </summary>
    private Transform DetectPlayerStructure()
    {
        if (Physics.Raycast(player.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 3f, structureLayerMask))
        {
            if (hit.collider.CompareTag("Untagged"))
            {
                return hit.transform;
            }
        }
        return null;
    }

    /// <summary>
    /// 지정된 구조물로 점프할 수 있는지 판단합니다.
    /// </summary>
    private bool CanJumpToStructure(Transform structure)
    {
        if (structure == null) return false;

        Collider structureCollider = structure.GetComponent<Collider>();
        if (structureCollider == null) return false;

        // 높이 계산
        float[] heights = {
            player.position.y - transform.position.y,
            structureCollider.bounds.max.y - transform.position.y,
            structureCollider.ClosestPoint(transform.position).y - transform.position.y,
            GetStructureSurfaceHeight(player.position) - transform.position.y
        };
        float chosenHeight = float.MaxValue;
        foreach (float h in heights)
        {
            if (h > 0.3f && h < chosenHeight) chosenHeight = h;
        }
        if (chosenHeight == float.MaxValue) return false;

        // 수평 거리 계산
        float horizontalDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(player.position.x, 0, player.position.z));

        if (horizontalDistance > 6f || !(chosenHeight <= (maxJumpHeight * 1.2f) && chosenHeight >= 0.3f))
        {
            return false;
        }

        jumpTargetPoint = new Vector3(player.position.x, player.position.y + 0.5f, player.position.z);
        return true;
    }

    /// <summary>
    /// 플레이어가 서있는 구조물의 정확한 표면 높이를 구합니다.
    /// </summary>
    private float GetStructureSurfaceHeight(Vector3 playerPos)
    {
        if (Physics.Raycast(playerPos + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 3f, structureLayerMask))
        {
            return hit.point.y;
        }
        return playerPos.y;
    }

    /// <summary>
    /// 스마트 점프를 실행합니다.
    /// </summary>
    private IEnumerator ExecuteSmartJump()
    {
        anim?.SetTrigger(jumpTrigger);

        Vector3 jumpDirection = (jumpTargetPoint - transform.position).normalized;
        jumpDirection.y = 0;
        verticalVelocity.y = jumpForce;

        float jumpStartTime = Time.time;
        while (Time.time - jumpStartTime < 3f && !CheckGroundStatus())
        {
            controller.Move(jumpDirection * moveSpeed * 0.8f * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        isJumping = false;
        currentTargetStructure = null;
    }

    /// <summary>
    /// 일반적인 이동 (점프하지 않을 때)
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (player == null) return;

        Vector3 targetOnPlane = new Vector3(player.position.x, transform.position.y, player.position.z);
        if (Vector3.Distance(transform.position, targetOnPlane) > stoppingDistance)
        {
            Vector3 moveDirection = (targetOnPlane - transform.position).normalized;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 5f);
            }
            anim?.SetBool(runningBool, true);
        }
        else
        {
            anim?.SetBool(runningBool, false);
        }
    }

    /// <summary>
    /// 땅에 닿아있는지 확인
    /// </summary>
    private bool CheckGroundStatus()
    {
        return Physics.Raycast(transform.position + controller.center, Vector3.down, controller.height / 2 + groundCheckDistance);
    }

    /// <summary>
    /// 중력 처리
    /// </summary>
    private void HandleGravity()
    {
        bool isGrounded = CheckGroundStatus();
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    public void ResetAIAndTeleport(Vector3 position, Quaternion rotation)
    {
        // CharacterController를 잠시 비활성화하여 위치를 강제로 설정합니다.
        controller.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        controller.enabled = true;

        // 모든 내부 상태 변수 초기화
        isJumping = false;
        wasGroundedLastFrame = true;
        verticalVelocity = Vector3.zero;
        lastJumpTime = -99f;
        currentTargetStructure = null;
        jumpTargetPoint = Vector3.zero;

        // 애니메이터 상태 초기화
        if (anim != null)
        {
            anim.SetBool(runningBool, false);
            anim.SetBool(isAirborneBool, false);
            anim.SetBool(isFallingBool, false);
            // 모든 트리거를 리셋하여 의도치 않은 애니메이션 방지
            anim.ResetTrigger(jumpTrigger);
            anim.ResetTrigger(landTrigger);
        }
    }
}
