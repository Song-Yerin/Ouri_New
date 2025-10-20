using Controller;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WallHang : MonoBehaviour
{
    [Header("벽 감지 설정")]
    [Tooltip("벽을 감지할 LayerMask")]
    public LayerMask wallLayer;
    [Tooltip("벽 감지 거리")]
    public float wallCheckDistance = 1f;
    [Tooltip("벽 감지를 시작할 플레이어 앞쪽 오프셋")]
    public Vector3 wallCheckOffset = new Vector3(0, 1f, 0);

    [Header("매달리기 입력")]
    [Tooltip("매달리기 활성화 키")]
    public KeyCode hangKey = KeyCode.F;

    [Header("점프 설정")]
    [Tooltip("매달린 상태에서 점프할 때 사용할 키")]
    public KeyCode jumpKey = KeyCode.Space;
    [Tooltip("점프 대체 입력 버튼 이름")]
    public string jumpButtonName = "Jump";
    [Tooltip("벽에서 점프할 때의 상승 속도")]
    public float wallJumpUpwardForce = 10f;
    [Tooltip("벽에서 점프할 때의 뒤쪽(벽 반대 방향) 속도")]
    public float wallJumpBackwardForce = 5f;

    [Header("애니메이션 설정")]
    [Tooltip("플레이어의 Animator 컴포넌트")]
    public Animator animator;
    [Tooltip("매달림 상태를 제어할 Bool 파라미터 이름")]
    public string hangParameterName = "IsHanging";

    // 내부 상태 변수
    private CharacterController controller;
    private CreatureMover creatureMover;

    private bool isHanging = false;
    private bool canHangAgain = true;
    private Vector3 hangPosition;
    private Vector3 wallNormal;
    private Vector3 jumpVelocity; // 점프 후 속도를 저장

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        creatureMover = GetComponent<CreatureMover>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        // 땅에 닿으면 재매달림 가능 상태로 초기화
        if (controller.isGrounded && !canHangAgain)
        {
            canHangAgain = true;
        }

        // 매달리기 입력 감지
        if (Input.GetKeyDown(hangKey) && !isHanging && canHangAgain)
        {
            TryHang();
        }

        // 매달린 상태에서 점프 입력 감지
        if (isHanging)
        {
            bool jumpPressed = Input.GetKeyDown(jumpKey);

            if (!jumpPressed && !string.IsNullOrEmpty(jumpButtonName))
            {
                try
                {
                    jumpPressed = Input.GetButtonDown(jumpButtonName);
                }
                catch { }
            }

            if (jumpPressed)
            {
                JumpFromWall();
            }

            // 매달린 상태 유지
            MaintainHangPosition();
        }
    }

    private void FixedUpdate()
    {
        // 점프 직후에만 WallHang이 속도를 적용
        if (!isHanging && jumpVelocity != Vector3.zero)
        {
            controller.Move(jumpVelocity * Time.fixedDeltaTime);

            // 중력 적용
            jumpVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;

            // CreatureMover가 다시 제어권을 가져가면 속도 초기화
            if (creatureMover != null && creatureMover.enabled)
            {
                jumpVelocity = Vector3.zero;
            }
        }
    }

    private void TryHang()
    {
        Vector3 rayOrigin = transform.position + wallCheckOffset;
        Vector3 rayDirection = transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, wallCheckDistance, wallLayer))
        {
            isHanging = true;
            hangPosition = transform.position;
            wallNormal = hit.normal;

            // CreatureMover 비활성화
            if (creatureMover != null)
            {
                creatureMover.enabled = false;
            }

            // 매달림 애니메이션 재생
            if (animator != null)
            {
                animator.SetBool(hangParameterName, true);
            }

            Debug.Log("벽에 매달렸습니다!");
        }
    }

    private void MaintainHangPosition()
    {
        controller.Move(Vector3.zero);
        transform.position = hangPosition;
    }

    /// <summary>
    /// 벽에서 점프 - CreatureMover 대신 직접 속도 부여
    /// </summary>
    private void JumpFromWall()
    {
        isHanging = false;
        canHangAgain = false;

        // 매달림 애니메이션 종료
        if (animator != null)
        {
            animator.SetBool(hangParameterName, false);
        }

        // --- [핵심 수정] 점프 속도를 직접 계산 ---
        // 위쪽 방향 + 벽 반대 방향(wallNormal)으로 속도 부여
        jumpVelocity = Vector3.up * wallJumpUpwardForce + wallNormal * wallJumpBackwardForce;

        Debug.Log($"벽에서 점프! 초기 속도: {jumpVelocity}");

        // 약간의 딜레이 후 CreatureMover 재활성화
        StartCoroutine(ReenableCreatureMoverAfterDelay());
    }

    /// <summary>
    /// 점프 직후 잠깐 대기한 후 CreatureMover 재활성화
    /// </summary>
    private System.Collections.IEnumerator ReenableCreatureMoverAfterDelay()
    {
        // 0.1초 대기 (점프 모션이 시작될 시간 확보)
        yield return new WaitForSeconds(0.1f);

        if (creatureMover != null)
        {
            creatureMover.enabled = true;
            Debug.Log("CreatureMover 재활성화!");
        }
    }

    public bool IsHanging => isHanging;

    private void OnDrawGizmos()
    {
        Vector3 rayOrigin = transform.position + wallCheckOffset;
        Vector3 rayDirection = transform.forward * wallCheckDistance;

        Gizmos.color = isHanging ? Color.green : Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection);
        Gizmos.DrawWireSphere(rayOrigin + rayDirection, 0.1f);
    }
}
