using System;
using UnityEngine;

namespace Controller
{
    // --- [수정] 모든 클래스를 독립적으로 분리 ---
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CreatureMover : MonoBehaviour
    {
        #region 변수 선언부 (수정 없음)
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private float m_GroundCheckDistance = 0.3f;
        [SerializeField] private float m_GroundCheckRadius = 0.4f;
        [Header("Movement")]
        [SerializeField] private float m_WalkSpeed = 1f;
        [SerializeField] private float m_RunSpeed = 4f;
        [SerializeField, Range(0f, 360f)] private float m_RotateSpeed = 90f;
        [SerializeField] private Space m_Space = Space.Self;
        [SerializeField] private float m_JumpHeight = 5f;
        [SerializeField] private float m_GlideGravity = -1f;
        //추가
        [Header("Glide Bonus")]
        [SerializeField] private float m_GlideBonusDamp = 2.5f;
        [SerializeField] private float m_GlideMaxBonusSpeed = 500f;
        private Vector3 m_GlideBonusVelocity;
        [SerializeField] private bool m_GlideAlignInstant = true;
        [SerializeField] private float m_GlideAlignYawDegPerSec = 720f;
        [SerializeField] private bool m_GlidePitchAffectsVertical = true;
        [SerializeField] private float m_GlidePitchUpAccel = 6f;
        [SerializeField] private float m_GlidePitchDownAccel = 10f;
        [SerializeField] private float m_GlidePitchMaxUpSpeed = 4f;
        [SerializeField] private float m_GlidePitchMaxDownSpeed = 12f;
        private float m_GlidePitchVelY;
        [Header("Animator")]
        [SerializeField] private string m_VerticalID = "Vert";
        [SerializeField] private string m_StateID = "State";
        [SerializeField] private string m_SlidingID = "IsSliding";
        [SerializeField] private string m_JumpTriggerID = "Jump";
        [SerializeField] private string m_IsGlidingID = "IsGliding";
        [SerializeField] private string m_IsGroundedID = "IsGrounded";
        [SerializeField] private string m_IsClimbingID = "IsClimbing";
        [SerializeField] private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);
        [Header("IK Settings")]
        [SerializeField] private bool m_UseIk = true;
        [SerializeField] private float m_IkSmoothSpeed = 10f;
        [Header("Visuals")]
        [SerializeField] private Transform m_RootBone;
        [SerializeField] private float m_VisualRotationSpeed = 15f;
        #endregion

        // --- 내부 상태 변수들 ---
        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;
        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;
        private PlayerCam m_PlayerCamera;
        private Vector2 m_Axis;
        private Vector3 m_Target;
        public bool m_IsRun;
        private bool m_IsMoving;
        private bool m_IsGlide = false;
        public bool IsGliding => m_IsGlide;
        private bool _glideToggleRequested = false;
        private bool m_IsSliding = false;
        private bool m_IsClimbing = false;
        private Vector3 m_ClimbWallNormal;
        private Vector3 m_SmoothedLookAtPos;
        private Vector2 _currentAnimAxis;
        private bool _isActuallyGrounded;
        private bool _jumpRequested = false;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);
            // Awake 전에는 m_Movement가 null일 수 있으므로 null 체크 추가
            m_Movement?.SetNormalMovementStats(m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_GlideGravity, m_Space);
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();
            m_Animator.applyRootMotion = false;
            if (Camera.main != null)
            {
                m_PlayerCamera = Camera.main.GetComponent<PlayerCam>();
            }

            m_Animation = new AnimationHandler(m_Animator, m_VerticalID, m_StateID, m_SlidingID, m_JumpTriggerID, m_IsGroundedID, m_IsGlidingID, m_IsClimbingID);
            m_Movement = new MovementHandler(m_Controller, m_Transform, m_Animation, m_GroundLayer, m_GroundCheckDistance, m_GroundCheckRadius);
            m_Movement.SetNormalMovementStats(m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_GlideGravity, m_Space);

            m_SmoothedLookAtPos = m_Transform.position + m_Transform.forward;
        }

        private void Update()
        {
            // Update 로직은 기존과 동일 : 흔들리는거때매 옮김
            //m_SmoothedLookAtPos = Vector3.Lerp(m_SmoothedLookAtPos, m_Target, Time.deltaTime * m_IkSmoothSpeed);
        }

        private void FixedUpdate()
        {
            // --- Glide 로직 ---
            if (_glideToggleRequested && !m_Controller.isGrounded) { m_IsGlide = !m_IsGlide; }
            _glideToggleRequested = false;
            if (m_Controller.isGrounded) { m_IsGlide = false; }

            // 지상에 닿으면 글라이드 무조건 해제 (추가)
            if (_isActuallyGrounded)
            {
                m_IsGlide = false;
                m_GlideBonusVelocity = Vector3.zero;
                m_GlidePitchVelY = 0f;
            }

            // --- 이동 처리 ---
            m_Movement.Move(Time.fixedDeltaTime, m_Axis, m_IsRun, _jumpRequested, m_IsMoving, m_IsGlide, m_IsClimbing, m_IsSliding, out _currentAnimAxis);
            _isActuallyGrounded = m_Controller.isGrounded;
            _jumpRequested = false;

            //추가
            if (m_IsGlide)
            {
                AlignGlideToView(Time.fixedDeltaTime);
                UpdateGlidePitchVertical(Time.fixedDeltaTime);

                if (m_GlideBonusVelocity.sqrMagnitude > 1e-6f || Mathf.Abs(m_GlidePitchVelY) > 1e-4f)
                {
                    Vector3 bonusStep = new Vector3(
                        m_GlideBonusVelocity.x,
                        m_GlidePitchVelY,
                        m_GlideBonusVelocity.z
                    ) * Time.fixedDeltaTime;

                    m_Controller.Move(bonusStep);

                    float dropH = m_GlideBonusDamp * Time.fixedDeltaTime;
                    m_GlideBonusVelocity = Vector3.MoveTowards(m_GlideBonusVelocity, Vector3.zero, dropH);
                }
            }

            if (m_IsSliding)
            {
                Vector3 slideDir = m_Controller.velocity;
                if (slideDir.sqrMagnitude > 0.01f)
                {
                    // y축만 회전 유지할 때
                    Vector3 flatDir = new Vector3(slideDir.x, 0, slideDir.z);
                    if (flatDir.sqrMagnitude > 0.01f)
                    {
                        Quaternion target = Quaternion.LookRotation(flatDir, Vector3.up);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation, target, Time.fixedDeltaTime * 10f // 10은 회전 속도, 기호에 따라 조정
                        );
                    }
                }

            }
        }

        private void LateUpdate()
        {
            // LateUpdate 로직은 기존과 동일
            m_Animation.Animate(in _currentAnimAxis, m_IsRun ? 1f : 0f, _isActuallyGrounded, m_IsGlide, m_IsClimbing, Time.deltaTime);
            ApplyVisualRootBoneRotation();
            //흔들림 보간
            float smoothFactor = 1.0f - Mathf.Pow(0.5f, Time.deltaTime * m_IkSmoothSpeed);
            m_SmoothedLookAtPos = Vector3.Lerp(m_SmoothedLookAtPos, m_Target, smoothFactor);
        }

        private void OnAnimatorIK()
        {
            // OnAnimatorIK 로직은 기존과 동일
            if (!m_UseIk || m_IsSliding || m_IsClimbing)
            {
                m_Animator.SetLookAtWeight(0);
                return;
            }
            m_Animation.AnimateIK(in m_SmoothedLookAtPos, m_LookWeight);
        }

        // --- [핵심 추가] Sliding.cs에서 호출할 함수들 ---
        public void StartNewSlideMode(Vector3 slideNormal, float friction, float gravityForce, float controlForce)
        {
            if (m_IsClimbing) return;
            m_IsSliding = true;
            m_Animation.SetSliding(true);
            m_Movement.EnterSlideState(slideNormal, friction, gravityForce, controlForce, m_Controller.velocity);
            m_IsRun = false; m_IsMoving = false;
        }
        public void StopNewSlideMode()
        {
            m_IsSliding = false;
            m_Animation.SetSliding(false);
            m_Movement.ExitSlideState();
        }
        // ---------------------------------------------

        //추가
        public void AddGlideImpulse(Vector3 worldDir, float impulse)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 1e-4f) return;
            worldDir.Normalize();
            m_GlideBonusVelocity += worldDir * impulse;
            m_GlideBonusVelocity = Vector3.ClampMagnitude(m_GlideBonusVelocity, m_GlideMaxBonusSpeed);
        }

        private void AlignGlideToView(float dt)
        {
            if (!m_IsGlide) return;
            if (m_GlideBonusVelocity.sqrMagnitude < 1e-4f) return;

            var cam = Camera.main ? Camera.main.transform : null;
            if (!cam) return;

            Vector3 desired = cam.forward; desired.y = 0f;
            if (desired.sqrMagnitude < 1e-4f) return;
            desired.Normalize();

            float speed = new Vector2(m_GlideBonusVelocity.x, m_GlideBonusVelocity.z).magnitude;
            if (speed < 1e-4f) return;

            Vector3 current = new Vector3(m_GlideBonusVelocity.x, 0f, m_GlideBonusVelocity.z).normalized;
            Vector3 newDir;
            if (m_GlideAlignInstant) newDir = desired;
            else
            {
                float maxRad = m_GlideAlignYawDegPerSec * Mathf.Deg2Rad * dt;
                newDir = Vector3.RotateTowards(current, desired, maxRad, float.MaxValue);
            }
            m_GlideBonusVelocity = newDir * speed;
        }

        private void UpdateGlidePitchVertical(float dt)
        {
            if (!m_GlidePitchAffectsVertical) return;
            if (!m_IsGlide) { m_GlidePitchVelY = 0f; return; }

            if (m_GlideBonusVelocity.sqrMagnitude < 1e-4f)
            {
                m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, 0f, m_GlidePitchDownAccel * dt);
                return;
            }

            var cam = Camera.main ? Camera.main.transform : null;
            if (!cam)
            {
                m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, 0f, m_GlidePitchDownAccel * dt);
                return;
            }

            float py = Mathf.Clamp(cam.forward.y, -1f, 1f);
            float targetYSpeed; float accel;

            if (py > 0f)
            {
                targetYSpeed = m_GlidePitchMaxUpSpeed * py;
                accel = m_GlidePitchUpAccel;
            }
            else if (py < 0f)
            {
                targetYSpeed = -m_GlidePitchMaxDownSpeed * (-py);
                accel = m_GlidePitchDownAccel;
            }
            else
            {
                targetYSpeed = 0f;
                accel = m_GlidePitchDownAccel;
            }

            m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, targetYSpeed, accel * dt);
        }
        #region 기타 메서드 (기존과 거의 동일)
        public void RequestJump() { _jumpRequested = true; }
        public void RequestGlideToggle() { _glideToggleRequested = true; }
        public bool IsActuallyGrounded => _isActuallyGrounded;

        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in Vector2 mouseDelta, float scroll)
        {
            m_PlayerCamera?.SetInput(mouseDelta, scroll);
            m_Target = target;
            m_IsRun = isRun;

            if (m_IsClimbing || m_IsSliding) { m_Axis = axis; } // 등반, 슬라이드 중에도 입력은 받도록 수정
            else { m_Axis = axis; }

            if (m_Axis.sqrMagnitude < Mathf.Epsilon) { m_IsMoving = false; }
            else { m_IsMoving = true; }
        }

        public void SetClimbMode(bool isClimbing, Vector3 wallNormal)
        {
            if (isClimbing && m_IsSliding) StopNewSlideMode();

            m_IsClimbing = isClimbing;
            m_ClimbWallNormal = wallNormal;
            m_Movement.SetClimbState(isClimbing, wallNormal);
            m_Animation.SetClimbing(isClimbing);

            if (isClimbing) { m_IsRun = false; m_IsMoving = false; }
        }

        private void ApplyVisualRootBoneRotation()
        {
            if (m_RootBone == null) return;
            Quaternion targetLocalRotation;
            if (m_IsClimbing)
            {
                Vector3 newForward = Vector3.down; Vector3 newUp = -m_ClimbWallNormal;
                Quaternion targetWorldRotation = Quaternion.LookRotation(newForward, newUp);
                targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
            }
            else if (m_IsSliding)
            {
                Vector3 slideNormal = m_Movement.GetCurrentSlideNormal();
                Vector3 slideForward = Vector3.ProjectOnPlane(transform.forward, slideNormal).normalized;
                Vector3 slideUp = slideNormal;
                Quaternion targetWorldRotation = Quaternion.LookRotation(slideForward, slideUp);
                targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
            }
            else { targetLocalRotation = Quaternion.identity; }
            m_RootBone.localRotation = Quaternion.Slerp(m_RootBone.localRotation, targetLocalRotation, Time.deltaTime * m_VisualRotationSpeed);
        }
        #endregion
    }

    [Serializable]
    public struct LookWeight
    {
        public float weight; public float body; public float head; public float eyes;
        public LookWeight(float weight, float body, float head, float eyes) { this.weight = weight; this.body = body; this.head = head; this.eyes = eyes; }
    }

    public class MovementHandler
    {
        private CharacterController _controller;
        private Transform _transform;
        private AnimationHandler _animation;

        // 땅 판정
        private LayerMask _groundLayer;
        private float _groundCheckDistance;
        private float _groundCheckRadius;

        // 일반 이동
        private float _walkSpeed, _runSpeed, _rotateSpeed, _jumpHeight, _glideGravity;
        private Vector3 _normalMoveVelocity;
        private Space _space; // 추가
        private Vector3 _lastForward; // 추가
        private float _targetAngle; // 추가
        private bool _isRotating; // 추가

        // 등반
        private bool _isClimbing;
        private Vector3 _climbNormal;

        // 슬라이드
        private Vector3 _slideVelocity;
        private Vector3 _slideNormal;
        private float _slideFriction;
        private float _slideGravityForce;
        private float _slideControlForce;

        public MovementHandler(CharacterController c, Transform t, AnimationHandler a, LayerMask gl, float dist, float radius)
        {
            _controller = c; _transform = t; _animation = a;
            _groundLayer = gl; _groundCheckDistance = dist; _groundCheckRadius = radius;
            _lastForward = t.forward;
        }

        public void SetNormalMovementStats(float w, float r, float rot, float j, float g, Space space)
        {
            _walkSpeed = w; _runSpeed = r; _rotateSpeed = rot; _jumpHeight = j; _glideGravity = g; _space = space;
        }

        public void SetClimbState(bool isClimbing, Vector3 normal) { _isClimbing = isClimbing; _climbNormal = normal; }
        public void EnterSlideState(Vector3 normal, float friction, float gravity, float control, Vector3 initialVelocity)
        {
            _slideNormal = normal; _slideFriction = friction; _slideGravityForce = gravity; _slideControlForce = control; _slideVelocity = initialVelocity;
        }
        public void ExitSlideState() { }
        public Vector3 GetCurrentSlideNormal() => _slideNormal;

        public void Move(float deltaTime, Vector2 axis, bool isRun, bool isJump, bool isMoving, bool isGlide, bool isClimbing, bool isSliding, out Vector2 animAxis)
        {
            if (isSliding) SlideMove(deltaTime, axis, out animAxis);
            else if (isClimbing) ClimbMove(deltaTime, axis, isRun, out animAxis);
            else NormalMove(deltaTime, axis, isRun, isJump, isMoving, isGlide, out animAxis);
        }

        private void NormalMove(float deltaTime, Vector2 axis, bool isRun, bool isJump, bool isMoving, bool isGlide, out Vector2 animAxis)
        {
            if (_controller.isGrounded)
            {
                _normalMoveVelocity.y = -2f;
                if (isJump) { _normalMoveVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y); _animation.TriggerJump(); }
            }
            else
            {
                if (isGlide && _normalMoveVelocity.y < 0) _normalMoveVelocity.y = _glideGravity;
                else _normalMoveVelocity.y += Physics.gravity.y * deltaTime;
            }

            Transform camTransform = Camera.main.transform;
            Vector3 forward = camTransform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = camTransform.right; right.y = 0; right.Normalize();
            Vector3 moveDirection = (axis.x * right + axis.y * forward).normalized;

            float targetSpeed = isRun ? _runSpeed : _walkSpeed;
            Vector3 horizontalMove = moveDirection * targetSpeed;

            _normalMoveVelocity.x = horizontalMove.x;
            _normalMoveVelocity.z = horizontalMove.z;

            if (isMoving)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, toRotation, _rotateSpeed * deltaTime);
            }

            _controller.Move(_normalMoveVelocity * deltaTime);
            animAxis = new Vector2(Vector3.Dot(moveDirection, _transform.right), Vector3.Dot(moveDirection, _transform.forward));
            animAxis *= (isRun ? 2f : 1f);
        }

        private void ClimbMove(float deltaTime, Vector2 axis, bool isRun, out Vector2 animAxis)
        {
            Vector3 wallUp = Vector3.up; Vector3 wallRight = Vector3.Cross(_climbNormal, wallUp).normalized;
            float currentSpeed = isRun ? _runSpeed : _walkSpeed;
            Vector3 movement = (wallUp * axis.y + wallRight * axis.x) * currentSpeed;
            _controller.Move(movement * deltaTime);
            animAxis = axis;
        }

        private void SlideMove(float deltaTime, Vector2 axis, out Vector2 animAxis)
        {
            // 1. 밑에 있는 땅의 Normal을 매 프레임 갱신 (SphereCast 등 활용)
            Vector3 rayOrigin = _transform.position + _controller.center;
            float rayLength = (_controller.height / 2f) + 0.1f;
            Vector3 updatedNormal = _slideNormal; // 기본값은 기존 normal

            if (Physics.SphereCast(rayOrigin, _controller.radius, Vector3.down, out RaycastHit hit, rayLength, LayerMask.GetMask("Default","Slide")))
            {
                updatedNormal = hit.normal;
            }
            _slideNormal = updatedNormal;

            // 2. 실제 슬라이드 물리 처리 (기존과 동일)
            if (!_controller.isGrounded) { _slideVelocity.y += Physics.gravity.y * deltaTime; }
            else { _slideVelocity.y = Mathf.Max(_slideVelocity.y, -2f); }

            Vector3 slopeForce = Vector3.ProjectOnPlane(Vector3.down, _slideNormal).normalized * _slideGravityForce;
            _slideVelocity += slopeForce * deltaTime;

            // 컨트롤 입력 및 기타 기존 코드...
            Transform camTransform = Camera.main.transform;
            Vector3 forward = camTransform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = camTransform.right; right.y = 0; right.Normalize();
            Vector3 controlDirection = (axis.x * right + axis.y * forward).normalized;
            Vector3 controlForce = controlDirection * _slideControlForce;
            _slideVelocity += controlForce * deltaTime;

            float yVel = _slideVelocity.y;
            Vector3 hVel = new Vector3(_slideVelocity.x, 0, _slideVelocity.z);
            hVel = Vector3.Lerp(hVel, Vector3.zero, _slideFriction * deltaTime);
            _slideVelocity = hVel + Vector3.up * yVel;

            _controller.Move(_slideVelocity * deltaTime);
            animAxis = new Vector2(axis.x, 1f);

        }
    }

    public class AnimationHandler
    {
        private readonly Animator m_Animator;
        private readonly string m_VerticalID, m_StateID, m_SlidingID, m_JumpTriggerID, m_IsGroundedID, m_IsGlidingID, m_IsClimbingID;
        private readonly float k_InputFlow = 4.5f;
        private float m_FlowState; private Vector2 m_FlowAxis;

        public AnimationHandler(Animator animator, string verticalID, string stateID, string slidingID, string jumpTriggerID, string isGroundedID, string isGlidingID, string isClimbingID)
        {
            m_Animator = animator; m_VerticalID = verticalID; m_StateID = stateID; m_SlidingID = slidingID; m_JumpTriggerID = jumpTriggerID; m_IsGroundedID = isGroundedID; m_IsGlidingID = isGlidingID; m_IsClimbingID = isClimbingID;
        }

        public void SetSliding(bool isSliding) { m_Animator.SetBool(m_SlidingID, isSliding); }
        public void SetClimbing(bool isClimbing) { m_Animator.SetBool(m_IsClimbingID, isClimbing); }
        public void TriggerJump() { m_Animator.SetTrigger(m_JumpTriggerID); }
        public void Animate(in Vector2 axis, float state, bool isGrounded, bool isGliding, bool isClimbing, float deltaTime)
        {
            m_Animator.SetBool(m_IsGroundedID, isGrounded);
            m_Animator.SetBool(m_IsGlidingID, isGliding);
            m_Animator.SetBool(m_IsClimbingID, isClimbing);
            m_FlowAxis = Vector2.Lerp(m_FlowAxis, axis, k_InputFlow * deltaTime);
            m_FlowState = Mathf.Lerp(m_FlowState, state, k_InputFlow * deltaTime);
            m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
            m_Animator.SetFloat(m_StateID, m_FlowState);
        }
        public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
        {
            m_Animator.SetLookAtPosition(target);
            m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
        }
    }


}
