using UnityEngine;
using Controller.CameraStates;

namespace Controller
{
    public class PlayerCam : MonoBehaviour
    {
        [Header("공통 타겟 설정")]
        [SerializeField]
        protected Transform m_Player;
        [SerializeField]
        protected Transform m_Target;
        [SerializeField]
        protected float TargetDistance = 5f;

        [Header("카메라 설정")]
        [Tooltip("카메라가 바라볼 플레이어의 위치 오프셋입니다.")]
        [SerializeField]
        private Vector3 m_LookAtOffset = new Vector3(0f, 1.5f, 0f);

        [Header("카메라 입력 감도")]
        [SerializeField]
        private Vector2 m_Sensitivity = new Vector2(3f, 2f);

        [Header("카메라 충돌 설정")]
        [Tooltip("카메라가 충돌을 감지할 레이어를 선택합니다. Player 레이어는 제외해주세요.")]
        [SerializeField]
        private LayerMask m_CollisionLayers = ~0;

        // --- [핵심 수정 1] ---
        [Tooltip("카메라의 충돌 감지 반경입니다. 이 값만큼 벽에서 미리 떨어집니다.")]
        [SerializeField]
        private float m_CollisionRadius = 0.2f; // 기존 Padding을 대체하는 더 정확한 방식
        // --------------------

        private ICameraState m_CurrentState;

        public Transform Player => m_Player;
        public Transform Target => m_Target;
        public Vector3 LookAtOffset => m_LookAtOffset;
        public Vector2 Angles { get; private set; }
        public float Distance { get; private set; }

        private Transform m_Transform;

        void Awake()
        {
            m_Transform = transform;

            if (m_Player == null)
            {
                Debug.LogError("PlayerCam에 플레이어가 할당되지 않았습니다!");
                this.enabled = false;
                return;
            }

            Vector3 lookOrigin = m_Player.position;
            Distance = Vector3.Distance(m_Transform.position, lookOrigin);
            Quaternion initialRotation = Quaternion.LookRotation(lookOrigin - m_Transform.position);
            Angles = new Vector2(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y);

            TransitionToState(new NormalCameraState());
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            m_CurrentState?.OnUpdate();
        }

        private void TransitionToState(ICameraState nextState)
        {
            m_CurrentState?.OnExit();
            m_CurrentState = nextState;
            m_CurrentState.OnEnter(this);
        }

        public virtual void SetInput(Vector2 delta, float scroll)
        {
            var angles = Angles;
            angles.y += delta.x * m_Sensitivity.x;
            angles.x -= delta.y * m_Sensitivity.y;
            angles.x = Mathf.Clamp(angles.x, -60f, 80f);
            Angles = angles;
        }

        // --- [핵심 수정 2] ---
        public void PositionCameraWithCollision(Vector3 desiredPosition, Vector3 lookPoint)
        {
            Vector3 pivotPoint = m_Player.position + LookAtOffset;
            Vector3 direction = desiredPosition - pivotPoint;
            float desiredDistance = direction.magnitude;

            // 수정: Raycast 대신 SphereCast를 사용하여 카메라의 부피를 감안한 충돌을 감지합니다.
            if (Physics.SphereCast(pivotPoint, m_CollisionRadius, direction.normalized, out RaycastHit hit, desiredDistance, m_CollisionLayers))
            {
                // 충돌이 감지되면, 카메라를 충돌 지점까지만 이동시킵니다.
                // SphereCast의 hit.distance는 표면까지의 거리이므로, 패딩이 필요 없습니다.
                m_Transform.position = pivotPoint + direction.normalized * hit.distance;
            }
            else
            {
                // 충돌이 없으면 원하는 위치로 그대로 이동합니다.
                m_Transform.position = desiredPosition;
            }

            m_Transform.LookAt(lookPoint);
        }
        // --------------------

        public void UpdateTargetObjectPosition()
        {
            if (m_Target == null) return;
            m_Target.position = m_Transform.position + m_Transform.forward * TargetDistance;
        }

        public void SetSlideState(bool isSliding)
        {
                TransitionToState(new NormalCameraState()); 
        }
    }
}
