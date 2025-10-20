using System;
using System.Collections;
using UnityEngine;

namespace Controller
{
    /// <summary>
    /// Rigidbody 기반 3인칭 캐릭터 컨트롤러
    /// 깃허브 인기 패턴을 참고한 안정적인 구조
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMover : MonoBehaviour
    {
        #region Inspector 설정
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float airControlStrength = 0.3f;
        [SerializeField] private float rotationSpeed = 15f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float maxSlopeAngle = 45f;

        [Header("Physics")]
        [SerializeField] private float gravityMultiplier = 2f;
        [SerializeField] private float groundDrag = 6f;
        [SerializeField] private float airDrag = 0f;

        [Header("Glide")]
        [SerializeField] private float glideGravity = -2f;
        [SerializeField] private float glideForwardSpeed = 5f;

        [Header("Animation")]
        [SerializeField] private string moveSpeedParam = "MoveSpeed";
        [SerializeField] private string isGroundedParam = "IsGrounded";
        [SerializeField] private string isGlidingParam = "IsGliding";
        [SerializeField] private string jumpTrigger = "Jump";

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        #endregion

        #region Private 변수
        private Rigidbody rb;
        private CapsuleCollider capsule;
        private Animator animator;

        private Vector2 moveInput;
        private bool jumpRequested;
        private bool isRunning;
        private bool isGliding;
        private bool glideToggleRequested;

        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float currentSpeed;

        private bool canMove = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            animator = GetComponent<Animator>();

            // Rigidbody 설정
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = false; // 커스텀 중력 사용

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            CheckGround();
            HandleGlideToggle();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            HandleMovement();
            ApplyDrag();

            jumpRequested = false;
        }
        #endregion

        #region Ground Detection
        private void CheckGround()
        {
            Vector3 origin = transform.position + Vector3.up * (capsule.radius + 0.1f);
            float checkDist = capsule.height / 2f - capsule.radius + groundCheckDistance;

            if (Physics.SphereCast(origin, capsule.radius * 0.9f, Vector3.down, out RaycastHit hit, checkDist, groundLayers))
            {
                isGrounded = true;
                groundNormal = hit.normal;

                // 착지 시 활공 해제
                if (isGliding)
                    isGliding = false;
            }
            else
            {
                isGrounded = false;
                groundNormal = Vector3.up;
            }
        }

        private float GetSlopeAngle()
        {
            return Vector3.Angle(groundNormal, Vector3.up);
        }

        private bool IsOnSteepSlope()
        {
            return isGrounded && GetSlopeAngle() > maxSlopeAngle;
        }
        #endregion

        #region Movement
        private void HandleMovement()
        {
            if (!canMove) return;

            if (isGrounded)
            {
                GroundMovement();
            }
            else
            {
                AirMovement();
            }

            RotateTowardsMovement();
        }

        private void GroundMovement()
        {
            Vector3 moveDirection = GetMovementDirection();

            if (IsOnSteepSlope())
            {
                // 가파른 경사면에서는 미끄러짐
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                Vector3 targetVelocity = slideDirection * 8f + moveDirection * (currentSpeed * 0.3f);

                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            }
            else
            {
                // 일반 지면 이동
                Vector3 targetVelocity = moveDirection * currentSpeed;

                // 경사면을 따라 이동
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal).normalized * currentSpeed;

                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            }

            // 점프
            if (jumpRequested && !IsOnSteepSlope())
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        private void AirMovement()
        {
            Vector3 moveDirection = GetMovementDirection();

            if (isGliding)
            {
                // 활공 중
                Vector3 glideVelocity = cameraTransform.forward * glideForwardSpeed;
                glideVelocity.y = glideGravity;
                rb.velocity = Vector3.Lerp(rb.velocity, glideVelocity, airControlStrength * Time.fixedDeltaTime * 2f);
            }
            else
            {
                // 일반 공중 제어
                Vector3 airAcceleration = moveDirection * currentSpeed * airControlStrength;
                rb.AddForce(airAcceleration, ForceMode.Acceleration);

                // 공중에서 최대 수평 속도 제한
                Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                if (horizontalVelocity.magnitude > currentSpeed)
                {
                    horizontalVelocity = horizontalVelocity.normalized * currentSpeed;
                    rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
                }
            }
        }

        private Vector3 GetMovementDirection()
        {
            if (moveInput.sqrMagnitude < 0.01f)
                return Vector3.zero;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return (forward * moveInput.y + right * moveInput.x).normalized;
        }

        private void RotateTowardsMovement()
        {
            Vector3 moveDirection = GetMovementDirection();

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        #endregion

        #region Physics
        private void ApplyGravity()
        {
            if (isGrounded)
            {
                // 지면에 붙어있도록
                if (rb.velocity.y < 0)
                    rb.velocity = new Vector3(rb.velocity.y, -2f, rb.velocity.z);
            }
            else if (!isGliding)
            {
                // 커스텀 중력
                rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
            }
        }

        private void ApplyDrag()
        {
            rb.drag = isGrounded ? groundDrag : airDrag;
        }
        #endregion

        #region Glide
        private void HandleGlideToggle()
        {
            if (glideToggleRequested && !isGrounded)
            {
                isGliding = !isGliding;
            }
            glideToggleRequested = false;
        }
        #endregion

        #region Animation
        private void UpdateAnimator()
        {
            if (animator == null) return;

            // 이동 속도
            float animSpeed = moveInput.magnitude * (isRunning ? 2f : 1f);
            animator.SetFloat(moveSpeedParam, animSpeed);

            // 지면 상태
            animator.SetBool(isGroundedParam, isGrounded);

            // 활공 상태
            animator.SetBool(isGlidingParam, isGliding);
        }
        #endregion

        #region Public API
        public void SetMovementInput(Vector2 input)
        {
            moveInput = input;
            currentSpeed = (moveInput.magnitude > 0.01f) ? (isRunning ? runSpeed : walkSpeed) : 0f;
        }

        public void SetRun(bool running)
        {
            isRunning = running;
            currentSpeed = (moveInput.magnitude > 0.01f) ? (isRunning ? runSpeed : walkSpeed) : 0f;
        }

        public void Jump()
        {
            jumpRequested = true;
            if (isGrounded && !IsOnSteepSlope())
            {
                animator?.SetTrigger(jumpTrigger);
            }
        }

        public void ToggleGlide()
        {
            glideToggleRequested = true;
        }

        public bool IsGrounded() => isGrounded;
        public bool IsGliding() => isGliding;

        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
        }
        #endregion
    }

    /// <summary>
    /// 간단한 입력 핸들러
    /// </summary>
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerInput : MonoBehaviour
    {
        private PlayerMover controller;

        private void Awake()
        {
            controller = GetComponent<PlayerMover>();
        }

        private void Update()
        {
            // 이동 입력
            Vector2 moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            controller.SetMovementInput(moveInput);

            // 달리기
            controller.SetRun(Input.GetKey(KeyCode.LeftShift));

            // 점프
            if (Input.GetButtonDown("Jump"))
            {
                if (!controller.IsGrounded())
                    controller.ToggleGlide();
                else
                    controller.Jump();
            }
        }
    }
}
