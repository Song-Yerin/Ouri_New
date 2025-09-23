using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스크립트가 CharacterController 컴포넌트를 요구하도록 설정
[RequireComponent(typeof(CharacterController))]
public class SetDestinationModule : MonoBehaviour
{
    /*
    [Header("추격 설정")]
    [SerializeField] float moveSpeed = 2f;
    [Tooltip("추격을 멈출 목표와의 최소 거리")]
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("상태 (디버깅용)")]
    [SerializeField] private Vector3 destination;
    public bool Chasing = false;
    public bool isFailed = false;
    public bool isSuccess = false;

    // 컴포넌트 참조
    private Animator anim;
    private CharacterController controller;
    [SerializeField] private StartChasingModule startChasingModule;

    // 중력 및 수직 속도
    private float gravity = -9.81f;
    private Vector3 verticalVelocity;


    void Start()
    {
        anim = GetComponent<Animator>();
        // 컴포넌트를 시작할 때 한 번만 가져옵니다.
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        if (isSuccess)
            return;

        if (isFailed)
        {
            enemyReset();
        }

        HandleGravity(); // 중력 처리 함수를 매 프레임 호출

        if (Chasing)
        {
            // 목표 지점의 Y값을 추격자 자신의 Y값으로 고정
            Vector3 targetOnPlane = new Vector3(destination.x, transform.position.y, destination.z);

            // 거리를 계산할 때도 Y축이 무시된 목표 지점을 사용
            float distance = Vector3.Distance(transform.position, targetOnPlane);

            if (distance > stoppingDistance)
            {
                // 이동 방향을 계산 (단위 벡터)
                Vector3 moveDirection = (targetOnPlane - transform.position).normalized;

                // --- [핵심 수정] ---
                // CharacterController.Move()를 사용하여 물리 충돌을 고려한 이동
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);

                // 추격자가 목표 지점을 바라보도록 부드럽게 회전
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
            else
            {
                // 도착했다고 판단
                Debug.Log("목적지 근처에 도달함");

                if (anim != null)
                    anim.SetBool("isRunning", false);

                Chasing = false;
                isFailed = true;
            }
        }
    }

    /// <summary>
    /// CharacterController에 중력을 적용하는 함수
    /// </summary>
    private void HandleGravity()
    {
        // 추격자가 땅에 있으면 수직 속도를 약간의 음수 값으로 리셋
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        // 중력을 수직 속도에 계속 더해줌
        verticalVelocity.y += gravity * Time.deltaTime;

        // 최종 수직 속도를 적용하여 이동
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    public void enemyReset()
    {
        startChasingModule.ResetPosition();
        StartCoroutine(startChasingModule.Reset());
        isFailed = false;
    }

    public void ResetAnim()
    {
        anim.SetBool("isRunning", true);
    }
    */
}
