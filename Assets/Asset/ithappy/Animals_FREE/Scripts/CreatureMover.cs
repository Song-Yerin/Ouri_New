using System;
using System.Collections;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CreatureMover : MonoBehaviour
    {
        #region 인스펙터 변수
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private float m_GroundCheckDistance = 0.3f;
        [SerializeField] private float m_GroundCheckRadius = 0.4f;

        [Header("Movement")]
        [SerializeField] private float m_WalkSpeed = 1f;
        [SerializeField] private float m_RunSpeed = 4f;
        [Tooltip("캐릭터가 이동 방향으로 회전하는 부드러움의 정도입니다. 높을수록 빠릅니다.")]
        [SerializeField, Range(1f, 30f)] private float m_RotationSmoothing = 15f;
        [SerializeField] private float m_JumpHeight = 5f;
        [SerializeField] private float m_GlideGravity = -1f;

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

        [Header("JumpMap Respawn")]
        [SerializeField] private Transform m_RespawnPoint;
        [SerializeField] private float m_DeathY = -30f;
        #endregion

        // 내부 변수
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
        private Vector3 _currentMoveDirection;
        private float _gravityMultiplier = 1f;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);
            // --- [핵심 수정] --- 불필요한 인자들을 모두 제거합니다.
            m_Movement?.SetNormalMovementStats(m_WalkSpeed, m_RunSpeed, m_JumpHeight, m_GlideGravity);
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
            // --- [핵심 수정] --- 불필요한 인자들을 모두 제거합니다.
            m_Movement.SetNormalMovementStats(m_WalkSpeed, m_RunSpeed, m_JumpHeight, m_GlideGravity);

            m_SmoothedLookAtPos = m_Transform.position + m_Transform.forward;
        }

        private void Update()
        {
            if (m_IsMoving && !m_IsSliding && !m_IsClimbing)
            {
                if (_currentMoveDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_currentMoveDirection, Vector3.up);
                    m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, targetRotation, Time.deltaTime * m_RotationSmoothing);
                }
            }
        }

        private void FixedUpdate()
        {
            if (_glideToggleRequested && !m_Controller.isGrounded) { m_IsGlide = !m_IsGlide; }
            _glideToggleRequested = false;
            if (m_Controller.isGrounded) { m_IsGlide = false; }
            if (_isActuallyGrounded)
            {
                m_IsGlide = false;
                m_GlideBonusVelocity = Vector3.zero;
                m_GlidePitchVelY = 0f;
            }

            m_Movement.Move(Time.fixedDeltaTime, m_Axis, m_IsRun, _jumpRequested, m_IsMoving, m_IsGlide, m_IsClimbing, m_IsSliding, _gravityMultiplier, out _currentAnimAxis, out _currentMoveDirection);
            _isActuallyGrounded = m_Controller.isGrounded;
            _jumpRequested = false;

            if (m_IsGlide)
            {
                AlignGlideToView(Time.fixedDeltaTime);
                UpdateGlidePitchVertical(Time.fixedDeltaTime);
                if (m_GlideBonusVelocity.sqrMagnitude > 1e-6f || Mathf.Abs(m_GlidePitchVelY) > 1e-4f)
                {
                    Vector3 bonusStep = new Vector3(m_GlideBonusVelocity.x, m_GlidePitchVelY, m_GlideBonusVelocity.z) * Time.fixedDeltaTime;
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
                    Vector3 flatDir = new Vector3(slideDir.x, 0, slideDir.z);
                    if (flatDir.sqrMagnitude > 0.01f)
                    {
                        Quaternion target = Quaternion.LookRotation(flatDir, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * 10f);
                    }
                }
            }
        }

        #region 나머지 함수들 (수정 없음)
        private void LateUpdate()
        {
            m_Animation.Animate(in _currentAnimAxis, m_IsRun ? 1f : 0f, _isActuallyGrounded, m_IsGlide, m_IsClimbing, Time.deltaTime);
            ApplyVisualRootBoneRotation();
            float smoothFactor = 1.0f - Mathf.Pow(0.5f, Time.deltaTime * m_IkSmoothSpeed);
            m_SmoothedLookAtPos = Vector3.Lerp(m_SmoothedLookAtPos, m_Target, smoothFactor);
        }

        private void OnAnimatorIK()
        {
            if (!m_UseIk || m_IsSliding || m_IsClimbing) { m_Animator.SetLookAtWeight(0); return; }
            m_Animation.AnimateIK(in m_SmoothedLookAtPos, m_LookWeight);
        }

        public void Bounce(Vector3 bounceVelocity)
        {
            m_Movement.SetVerticalVelocity(bounceVelocity.y);
            _jumpRequested = false;
            m_IsGlide = false;
            m_Animation.TriggerJump();
            Debug.Log($"Bounce applied: {bounceVelocity}");
        }

        public void Respawn()
        {
            if (m_RespawnPoint == null) { Debug.LogWarning("리스폰 장소 연결 필요"); return; }
            m_Controller.enabled = false;
            transform.position = m_RespawnPoint.position;
            transform.rotation = m_RespawnPoint.rotation;
            m_Controller.enabled = true;
            ResetKinetics(true, true, true);
            Debug.Log("respawned");
        }

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

        public void ResetKinetics(bool stopSlide = true, bool stopClimb = true, bool clearGlide = true)
        {
            _jumpRequested = false;
            m_IsMoving = false;
            if (clearGlide) { m_IsGlide = false; m_GlideBonusVelocity = Vector3.zero; m_GlidePitchVelY = 0f; }
            if (stopSlide && m_IsSliding) StopNewSlideMode();
            if (stopClimb && m_IsClimbing) SetClimbMode(false, Vector3.zero);
            m_Movement.ResetVelocities();
        }

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
            if (!m_IsGlide || m_GlideBonusVelocity.sqrMagnitude < 1e-4f) return;
            var cam = Camera.main ? Camera.main.transform : null;
            if (!cam) return;
            Vector3 desired = cam.forward; desired.y = 0f;
            if (desired.sqrMagnitude < 1e-4f) return;
            desired.Normalize();
            float speed = new Vector2(m_GlideBonusVelocity.x, m_GlideBonusVelocity.z).magnitude;
            if (speed < 1e-4f) return;
            Vector3 current = new Vector3(m_GlideBonusVelocity.x, 0f, m_GlideBonusVelocity.z).normalized;
            Vector3 newDir = m_GlideAlignInstant ? desired : Vector3.RotateTowards(current, desired, m_GlideAlignYawDegPerSec * Mathf.Deg2Rad * dt, float.MaxValue);
            m_GlideBonusVelocity = newDir * speed;
        }

        private void UpdateGlidePitchVertical(float dt)
        {
            if (!m_GlidePitchAffectsVertical) return;
            if (!m_IsGlide) { m_GlidePitchVelY = 0f; return; }
            if (m_GlideBonusVelocity.sqrMagnitude < 1e-4f) { m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, 0f, m_GlidePitchDownAccel * dt); return; }
            var cam = Camera.main ? Camera.main.transform : null;
            if (!cam) { m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, 0f, m_GlidePitchDownAccel * dt); return; }
            float py = Mathf.Clamp(cam.forward.y, -1f, 1f);
            float targetYSpeed = (py > 0f) ? m_GlidePitchMaxUpSpeed * py : -m_GlidePitchMaxDownSpeed * (-py);
            float accel = (py > 0f) ? m_GlidePitchUpAccel : m_GlidePitchDownAccel;
            m_GlidePitchVelY = Mathf.MoveTowards(m_GlidePitchVelY, targetYSpeed, accel * dt);
        }

        public void RequestJump() { _jumpRequested = true; }
        public void RequestGlideToggle() { _glideToggleRequested = true; }
        public bool IsActuallyGrounded => _isActuallyGrounded;

        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in Vector2 mouseDelta, float scroll)
        {
            m_PlayerCamera?.SetInput(mouseDelta, scroll);
            m_Target = target;
            m_IsRun = isRun;
            if (m_IsClimbing || m_IsSliding) { m_Axis = axis; } else { m_Axis = axis; }
            if (m_Axis.sqrMagnitude < Mathf.Epsilon) { m_IsMoving = false; } else { m_IsMoving = true; }
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
                Quaternion targetWorldRotation = Quaternion.LookRotation(Vector3.down, -m_ClimbWallNormal);
                targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
            }
            else if (m_IsSliding)
            {
                Vector3 slideNormal = m_Movement.GetCurrentSlideNormal();
                Vector3 slideForward = Vector3.ProjectOnPlane(transform.forward, slideNormal).normalized;
                Quaternion targetWorldRotation = Quaternion.LookRotation(slideForward, slideNormal);
                targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
            }
            else { targetLocalRotation = Quaternion.identity; }
            m_RootBone.localRotation = Quaternion.Slerp(m_RootBone.localRotation, targetLocalRotation, Time.deltaTime * m_VisualRotationSpeed);
        }

        public void ApplyTemporaryGravityMultiplier(float multiplier, float duration)
        {
            // 이전에 실행 중이던 중력 변경 코루틴이 있다면 중지
            StopCoroutine("TemporaryGravityCoroutine");
            StartCoroutine(TemporaryGravityCoroutine(multiplier, duration));
        }

        private IEnumerator TemporaryGravityCoroutine(float multiplier, float duration)
        {
            // 새로운 중력 계수 적용
            _gravityMultiplier = multiplier;

            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(duration);

            // 원래 중력 계수(1)로 복원
            _gravityMultiplier = 1f;
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
        private LayerMask _groundLayer;
        private float _groundCheckDistance, _groundCheckRadius;
        private float _walkSpeed, _runSpeed, _jumpHeight, _glideGravity;
        private Vector3 _normalMoveVelocity;
        private bool _isClimbing;
        private Vector3 _climbNormal;
        private Vector3 _slideVelocity, _slideNormal;
        private float _slideFriction, _slideGravityForce, _slideControlForce;

        private float m_JumpMoveSpeed = 25f;

        // --- [추가] 경사 미끄러짐 관련 변수 ---
        private float slopeCheckDistance = 1.5f; // 경사 체크 레이캐스트 거리
        private float slopeLimit = 80f; // 이 각도 이상이면 미끄러짐 (CharacterController의 slopeLimit과 동일하게 설정 권장)
        private float slopeSlideSpeed = 8f; // 경사를 미끄러지는 속도
        private bool isAutoSliding = false; // 현재 자동 미끄러짐 중인지 여부

        public MovementHandler(CharacterController c, Transform t, AnimationHandler a, LayerMask gl, float dist, float radius)
        {
            _controller = c;
            _transform = t;
            _animation = a;
            _groundLayer = gl;
            _groundCheckDistance = dist;
            _groundCheckRadius = radius;

            // CharacterController의 slopeLimit 값을 가져와 일치시키는 것이 좋습니다
            slopeLimit = _controller.slopeLimit;
        }

        public void ResetVelocities()
        {
            _normalMoveVelocity = Vector3.zero;
            _slideVelocity = Vector3.zero;
        }

        public void SetVerticalVelocity(float yVel) => _normalMoveVelocity.y = yVel;

        public void SetNormalMovementStats(float w, float r, float j, float g)
        {
            _walkSpeed = w;
            _runSpeed = r;
            _jumpHeight = j;
            _glideGravity = g;
        }

        public void SetClimbState(bool isClimbing, Vector3 normal)
        {
            _isClimbing = isClimbing;
            _climbNormal = normal;
        }

        public void EnterSlideState(Vector3 normal, float friction, float gravity, float control, Vector3 initialVelocity)
        {
            _slideNormal = normal;
            _slideFriction = friction;
            _slideGravityForce = gravity;
            _slideControlForce = control;
            _slideVelocity = initialVelocity;
        }

        public void ExitSlideState() { }
        public Vector3 GetCurrentSlideNormal() => _slideNormal;

        // --- [추가] 외부에서 자동 미끄러짐 상태를 확인할 수 있는 프로퍼티 ---
        public bool IsAutoSliding => isAutoSliding;

        // --- [수정] 경사면이 너무 가파른지 검사하는 함수 ---
        private bool CheckSteepSlope(out Vector3 slideDirection, out float slopeAngle)
        {
            slideDirection = Vector3.zero;
            slopeAngle = 0f;

            if (!_controller.isGrounded)
                return false;

            Vector3 origin = _transform.position + _controller.center;
            float maxDistance = _controller.height / 2f;

            // [핵심!] 발 주변 8방향으로 Raycast (360도 커버)
            Vector3[] directions = new Vector3[]
            {
        Vector3.down,                                    // 바로 아래
        Vector3.down + _transform.forward * 0.5f,        // 앞쪽 아래
        Vector3.down - _transform.forward * 0.5f,        // 뒤쪽 아래
        Vector3.down + _transform.right * 0.5f,          // 오른쪽 아래
        Vector3.down - _transform.right * 0.5f,          // 왼쪽 아래
        Vector3.down + (_transform.forward + _transform.right) * 0.3f,   // 대각선 1
        Vector3.down + (_transform.forward - _transform.right) * 0.3f,   // 대각선 2
        Vector3.down + (-_transform.forward + _transform.right) * 0.3f,  // 대각선 3
        Vector3.down + (-_transform.forward - _transform.right) * 0.3f   // 대각선 4
            };

            float steepestAngle = 0f;
            Vector3 steepestNormal = Vector3.up;
            bool foundSteepSlope = false;

            foreach (Vector3 dir in directions)
            {
                Vector3 normalizedDir = dir.normalized;

                if (Physics.Raycast(origin, normalizedDir, out RaycastHit hit, maxDistance, _groundLayer))
                {
                    float angle = Vector3.Angle(hit.normal, Vector3.up);

                    // 디버그 라인
                    Debug.DrawLine(origin, hit.point, angle > slopeLimit ? Color.red : Color.green);

                    // 가장 가파른 각도 저장
                    if (angle > steepestAngle)
                    {
                        steepestAngle = angle;
                        steepestNormal = hit.normal;
                    }

                    if (angle > slopeLimit)
                    {
                        foundSteepSlope = true;
                    }
                }
            }

            if (foundSteepSlope)
            {
                slopeAngle = steepestAngle;
                slideDirection = Vector3.ProjectOnPlane(Vector3.down, steepestNormal).normalized;

                // 미끄러짐 방향 시각화
                Debug.DrawRay(_transform.position, slideDirection * 3f, Color.yellow);

                return true;
            }

            return false;
        }


        // --- [추가] 구체를 그리는 헬퍼 함수 ---
        /// <summary>
        /// Scene 뷰에서 구체를 와이어프레임으로 그립니다.
        /// </summary>
        private void DrawDebugSphere(Vector3 center, float radius, Color color, int segments = 16)
        {
            // 수평 원 (XZ 평면)
            DrawDebugCircle(center, radius, Vector3.up, color, segments);

            // 수직 원 1 (XY 평면)
            DrawDebugCircle(center, radius, Vector3.forward, color, segments);

            // 수직 원 2 (YZ 평면)
            DrawDebugCircle(center, radius, Vector3.right, color, segments);
        }

        /// <summary>
        /// Scene 뷰에서 원을 그립니다.
        /// </summary>
        private void DrawDebugCircle(Vector3 center, float radius, Vector3 normal, Color color, int segments)
        {
            Vector3 from = Vector3.zero;
            float angleStep = 360f / segments;

            // 원의 첫 번째 점 계산
            Vector3 perpendicular = Vector3.Cross(normal, Vector3.up);
            if (perpendicular.sqrMagnitude < 0.001f)
            {
                perpendicular = Vector3.Cross(normal, Vector3.forward);
            }
            perpendicular.Normalize();

            from = center + perpendicular * radius;

            for (int i = 0; i <= segments; i++)
            {
                float angle = angleStep * i;
                Vector3 direction = Quaternion.AngleAxis(angle, normal) * perpendicular;
                Vector3 to = center + direction * radius;

                Debug.DrawLine(from, to, color);
                from = to;
            }
        }

        public void Move(float deltaTime, Vector2 axis, bool isRun, bool isJump, bool isMoving, bool isGlide, bool isClimbing, bool isSliding, float gravityMultiplier, out Vector2 animAxis, out Vector3 moveDirection)
        {
            moveDirection = Vector3.zero;

            if (isSliding)
                SlideMove(deltaTime, axis, out animAxis);
            else if (isClimbing)
                ClimbMove(deltaTime, axis, isRun, out animAxis);
            else
                NormalMove(deltaTime, axis, isRun, isJump, isMoving, isGlide, gravityMultiplier, out animAxis, out moveDirection);
        }

        private void NormalMove(float deltaTime, Vector2 axis, bool isRun, bool isJump, bool isMoving, bool isGlide, float gravityMultiplier, out Vector2 animAxis, out Vector3 moveDirection)
        {
            // --- [수정] 경사 미끄러짐 체크 (isGrounded 조건 제거) ---
            bool onSteepSlope = CheckSteepSlope(out Vector3 slopeSlideDir, out float slopeAngle);
            bool effectivelyGrounded = _controller.isGrounded && !onSteepSlope;
            // [디버깅] 경사 상태 확인
            if (onSteepSlope)
            {
                Debug.Log($"[NormalMove] 가파른 경사 위 - 각도: {slopeAngle:F1}도, 미끄러짐 방향: {slopeSlideDir}");
            }

            // [수정] isGrounded 체크 없이 경사만으로 판단
            isAutoSliding = onSteepSlope; 

            // 지면 체크 및 점프 처리
            if (effectivelyGrounded)
            {
                _normalMoveVelocity.y = -2f;

                // 가파른 경사면에서는 점프 불가
                if (isJump && !isAutoSliding)
                {
                    _normalMoveVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);
                    _animation.TriggerJump();
                }
            }
            else
            {
                // 공중에서의 중력 적용
                float currentGravity = Physics.gravity.y * gravityMultiplier;

                if (isGlide && _normalMoveVelocity.y < 0)
                    _normalMoveVelocity.y = _glideGravity;
                else
                    _normalMoveVelocity.y += currentGravity * deltaTime;
            }

            // 카메라 기준 이동 방향 계산
            Transform camTransform = Camera.main.transform;
            Vector3 forward = camTransform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = camTransform.right;
            right.y = 0;
            right.Normalize();
            moveDirection = (axis.x * right + axis.y * forward).normalized;

            // --- [수정] 경사 미끄러짐 적용 ---
            if (isAutoSliding)
            {
                Debug.Log($"[NormalMove] 미끄러짐 적용 중! 방향: {slopeSlideDir}");

                // 플레이어 입력과 미끄러짐 방향 혼합
                float playerControlStrength = 0.3f;
                Vector3 playerInfluence = moveDirection * (isRun ? _runSpeed : _walkSpeed) * playerControlStrength;
                Vector3 slopeInfluence = slopeSlideDir * slopeSlideSpeed;

                Vector3 combinedHorizontal = playerInfluence + slopeInfluence;

                // [디버깅] 최종 이동 벡터 출력
                Debug.Log($"[NormalMove] 최종 이동 벡터: {combinedHorizontal}");

                _normalMoveVelocity.x = combinedHorizontal.x;
                _normalMoveVelocity.z = combinedHorizontal.z;

                // [중요] Y축 속도는 경사를 따라 내려가도록 설정
                // 경사면을 따라 미끄러질 때는 약간의 하향 속도가 필요합니다
                if (_controller.isGrounded)
                {
                    _normalMoveVelocity.y = -5f; // 경사를 따라 내려가는 힘
                }
            }
            else
            {
                // 일반 이동 (기존 로직)
                float targetSpeed = isJump ? m_JumpMoveSpeed : (isRun ? _runSpeed : _walkSpeed);
                Vector3 horizontalMove = moveDirection * targetSpeed;
                _normalMoveVelocity.x = horizontalMove.x;
                _normalMoveVelocity.z = horizontalMove.z;
            }

            // 최종 이동 적용
            _controller.Move(_normalMoveVelocity * deltaTime);

            // 애니메이션용 축 계산
            animAxis = new Vector2(Vector3.Dot(moveDirection, _transform.right), Vector3.Dot(moveDirection, _transform.forward));
            animAxis *= (isRun ? 2f : 1f);
        }

        private void ClimbMove(float deltaTime, Vector2 axis, bool isRun, out Vector2 animAxis)
        {
            Vector3 wallUp = Vector3.up;
            Vector3 wallRight = Vector3.Cross(_climbNormal, wallUp).normalized;
            float currentSpeed = isRun ? _runSpeed : _walkSpeed;
            Vector3 movement = (wallUp * axis.y + wallRight * axis.x) * currentSpeed;
            _controller.Move(movement * deltaTime);
            animAxis = axis;
        }

        private void SlideMove(float deltaTime, Vector2 axis, out Vector2 animAxis)
        {
            Vector3 rayOrigin = _transform.position + _controller.center;
            float rayLength = (_controller.height / 2f) + 0.1f;

            if (Physics.SphereCast(rayOrigin, _controller.radius, Vector3.down, out RaycastHit hit, rayLength, LayerMask.GetMask("Default", "Slide")))
            {
                _slideNormal = hit.normal;
            }

            if (!_controller.isGrounded)
            {
                _slideVelocity.y += Physics.gravity.y * deltaTime;
            }
            else
            {
                _slideVelocity.y = Mathf.Max(_slideVelocity.y, -2f);
            }

            Vector3 slopeForce = Vector3.ProjectOnPlane(Vector3.down, _slideNormal).normalized * _slideGravityForce;
            _slideVelocity += slopeForce * deltaTime;

            Transform camTransform = Camera.main.transform;
            Vector3 forward = camTransform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = camTransform.right;
            right.y = 0;
            right.Normalize();
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
