using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SmartChaseAI : MonoBehaviour
{
    [Header("추격 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] public float jumpCooldown = 2.0f; // 점프 쿨타임
    [SerializeField] private LayerMask structureLayerMask = -1;

    [Header("구조물 감지")]
    [SerializeField] private float groundCheckDistance = 1f;

    [Header("장애물 점프 설정")]
    [SerializeField] private float obstacleDetectDistance = 1.0f;
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
    private float lastJumpTime = -99f; // 마지막 점프 시간 기록

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

        // --- [핵심 수정] ---
        // 땅에 있고, 쿨타임이 지났을 때만 점프를 판단합니다.
        if (isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            // 1. 전방 장애물 점프 판단
            if (DetectObstacle())
            {
                isJumping = true;
                lastJumpTime = Time.time; // 쿨타임 갱신
                StartCoroutine(ExecuteJumpOverObstacle());
                return;
            }

            // 2. 높은 곳의 플레이어를 향한 스마트 점프 판단
            if (AnalyzeAndTrySmartJump())
            {
                lastJumpTime = Time.time; // 쿨타임 갱신
                return;
            }
        }

        // 점프를 하지 않았다면, 일반 추격 이동을 수행합니다.
        MoveTowardsTarget();

        wasGroundedLastFrame = isGrounded;
    }

    /// <summary>
    /// 전방 장애물 감지
    /// </summary>
    private bool DetectObstacle()
    {
        Vector3 origin = transform.position + Vector3.up * obstacleDetectHeight;
        return Physics.Raycast(origin, transform.forward, obstacleDetectDistance, obstacleLayerMask);
    }

    /// <summary>
    /// 장애물 위로 점프 (간단한 버전)
    /// </summary>
    private IEnumerator ExecuteJumpOverObstacle()
    {
        anim?.SetTrigger(jumpTrigger);
        verticalVelocity.y = jumpForce;
        yield return new WaitForSeconds(1.0f);
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
    /// 점프를 시작했다면 true를 반환합니다.
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
                return true; // 점프 시작
            }
        }
        return false; // 점프하지 않음
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
