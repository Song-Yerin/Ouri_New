using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class AutoWalk : MonoBehaviour
{
    [Header("Target & Speed")]
    public Transform targetPosition;
    public float moveSpeed = 2f;          // 목표 걷기 속도
    public float stopDistance = 0.15f;    // 도착 판정 거리

    [Header("Ease")]
    public float accel = 4f;              // 가속 (m/s^2 비슷한 느낌)
    public float decel = 6f;              // 감속

    private Animator animator;
    private CharacterController controller;

    private bool isWalking = false;
    private bool isStopping = false;
    private float currentSpeed = 0f;      // 가감속 적용용

    // CreatureMover에서 쓰던 파라미터
    private static readonly int IsGroundID = Animator.StringToHash("IsGrounded");
    private static readonly int VertID = Animator.StringToHash("Vert");
    private static readonly int StateID = Animator.StringToHash("State");

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 걷지 않을 땐 애니메이터만 유지
        if (!isWalking || targetPosition == null)
        {
            // 중력 살짝 눌러 IsGrounded 유지
            controller.Move(new Vector3(0f, -2f, 0f) * Time.deltaTime);
            return;
        }

        /* ① 방향 계산 */
        Vector3 offset = targetPosition.position - transform.position;
        Vector3 horizontal = new Vector3(offset.x, 0f, offset.z);
        float dist = horizontal.magnitude;

        if (horizontal.sqrMagnitude > 0.0001f)
        {
            var look = Quaternion.LookRotation(horizontal);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }

        /* ② 속도(가감속) 계산 */
        float targetSpeed = isStopping ? 0f : moveSpeed;
        float rate = (isStopping ? decel : accel);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        /* ③ 이동 */
        Vector3 move = (horizontal.sqrMagnitude > 0.0001f ? horizontal.normalized : Vector3.zero) * currentSpeed;
        move.y = -2f; // 지면에 살짝 눌러 붙이기
        controller.Move(move * Time.deltaTime);

        /* ④ 애니메이터 파라미터 */
        animator.SetBool(IsGroundID, true);
        // 0~1 사이로 보간(걷기 블렌드)
        float vert = (moveSpeed <= 0.0001f) ? 0f : Mathf.Clamp01(currentSpeed / moveSpeed);
        animator.SetFloat(VertID, vert);

        // 기존 State 값 유지 (원 코드와 동일)
        float stateV = 0.5f + 0.3f * Mathf.PingPong(Time.time * 2f, 1f);
        animator.SetFloat(StateID, stateV);

        /* ⑤ 도착/정지 판정 */
        if (!isStopping && dist < stopDistance)
        {
            // 목적지에 도착하면 부드럽게 멈춤
            StopWalking(false);
        }

        if (isStopping && currentSpeed <= 0.001f)
        {
            // 완전히 정지
            isWalking = false;
            isStopping = false;
            currentSpeed = 0f;
            animator.SetFloat(VertID, 0f);
        }
    }

    /* 타임라인/이벤트용: 걷기 시작 */
    public void StartWalking()
    {
        isStopping = false;
        isWalking = true;
        // 출발 시 가속 느낌 주기 위해 현재속도 0으로
        currentSpeed = 0f;
    }

    /* 타임라인/이벤트용: 걷기 정지
       immediate=true  : 즉시 멈춤(애니메이션도 Idle)
       immediate=false : 부드럽게 감속하여 정지(기본) */
    public void StopWalking(bool immediate = false)
    {
        if (immediate)
        {
            isWalking = false;
            isStopping = false;
            currentSpeed = 0f;
            animator.SetFloat(VertID, 0f);
            return;
        }

        // 부드럽게 멈추기
        isStopping = true;
        isWalking = true; // 감속 과정은 계속 Update에서 처리
    }
}