using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class AutoWalk : MonoBehaviour
{
    [Header("Target & Speed")]
    public Transform targetPosition;
    public float moveSpeed = 2f;           // 원하는 걷기 속도

    private Animator animator;
    private CharacterController controller;

    private bool isWalking = false;

    // 애니메이터 파라미터 ID - CreatureMover에서 쓰던 것 그대로
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
        if (!isWalking || targetPosition == null) return;

        /* ── ① 방향 계산 ──────────────────────── */
        Vector3 offset = targetPosition.position - transform.position;
        Vector3 horizontal = new Vector3(offset.x, 0f, offset.z);
        float dist3D = offset.magnitude;

        if (horizontal.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(horizontal);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }

        /* ── ② 지면에 붙인 채 이동 (y 고정) ─── */
        Vector3 move = horizontal.normalized * moveSpeed;
        move.y = -2f;                       // 살짝 눌러 isGrounded 유지
        controller.Move(move * Time.deltaTime);

        /* ── ③ 애니메이션 파라미터 ──────────── */
        animator.SetBool(IsGroundID, true);    // 항상 지면 상태
        animator.SetFloat(VertID, 1f);

        float stateV = 0.5f + 0.3f * Mathf.PingPong(Time.time * 2f, 1f);
        animator.SetFloat(StateID, stateV);    // Blend Tree에서 ‘걷기’ 값
        

        /* ── ④ 도착 체크 ────────────────────── */
        if (dist3D < 0.15f)
        {
            isWalking = false;
            animator.SetFloat(VertID, 0f);      // Idle 로 전환
        }
    }

    /* 타임라인 시그널용 */
    public void StartWalking()
    {
        isWalking = true;
    }
}




