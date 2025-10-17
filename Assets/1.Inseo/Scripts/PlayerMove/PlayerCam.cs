using UnityEngine;
using Controller.CameraStates;

namespace Controller
{
    public class PlayerCam : MonoBehaviour
    {
        [Header("���� Ÿ�� ����")]
        [SerializeField]
        protected Transform m_Player;
        [SerializeField]
        protected Transform m_Target;
        [SerializeField]
        protected float TargetDistance = 5f;

        [Header("ī�޶� ����")]
        [Tooltip("ī�޶� �ٶ� �÷��̾��� ��ġ �������Դϴ�.")]
        [SerializeField]
        private Vector3 m_LookAtOffset = new Vector3(0f, 1.5f, 0f);

        [Header("ī�޶� �Է� ����")]
        [SerializeField]
        private Vector2 m_Sensitivity = new Vector2(3f, 2f);

        [Header("ī�޶� �浹 ����")]
        [Tooltip("ī�޶� �浹�� ������ ���̾ �����մϴ�. Player ���̾�� �������ּ���.")]
        [SerializeField]
        private LayerMask m_CollisionLayers = ~0;

        // --- [�ٽ� ���� 1] ---
        [Tooltip("ī�޶��� �浹 ���� �ݰ��Դϴ�. �� ����ŭ ������ �̸� �������ϴ�.")]
        [SerializeField]
        private float m_CollisionRadius = 0.2f; // ���� Padding�� ��ü�ϴ� �� ��Ȯ�� ���
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
                Debug.LogError("PlayerCam�� �÷��̾ �Ҵ���� �ʾҽ��ϴ�!");
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
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
        // �߰� 
        void LateUpdate()
        {
            var brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
            if (brain != null && (brain.ActiveVirtualCamera != null || brain.IsBlending))
                return;

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

        // --- [�ٽ� ���� 2] ---
        public void PositionCameraWithCollision(Vector3 desiredPosition, Vector3 lookPoint)
        {
            Vector3 pivotPoint = m_Player.position + LookAtOffset;
            Vector3 direction = desiredPosition - pivotPoint;
            float desiredDistance = direction.magnitude;

            // ����: Raycast ��� SphereCast�� ����Ͽ� ī�޶��� ���Ǹ� ������ �浹�� �����մϴ�.
            if (Physics.SphereCast(pivotPoint, m_CollisionRadius, direction.normalized, out RaycastHit hit, desiredDistance, m_CollisionLayers))
            {
                // �浹�� �����Ǹ�, ī�޶� �浹 ���������� �̵���ŵ�ϴ�.
                // SphereCast�� hit.distance�� ǥ������� �Ÿ��̹Ƿ�, �е��� �ʿ� �����ϴ�.
                m_Transform.position = pivotPoint + direction.normalized * hit.distance;
            }
            else
            {
                // �浹�� ������ ���ϴ� ��ġ�� �״�� �̵��մϴ�.
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

        public void UpdateRotation(Vector2 lookInput)
        {
            // 1. 기존 SetInput 메서드를 재사용하여 카메라 각도(Angles)를 갱신합니다.
            SetInput(lookInput, 0f);

            // 2. LateUpdate의 로직과 유사하게, 갱신된 각도를 실제 카메라 위치와 회전에 적용합니다.
            //    Cinemachine이 활성화되어 있으면 동작하지 않도록 방지합니다.
            var brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
            if (brain != null && (brain.ActiveVirtualCamera != null || brain.IsBlending))
                return;

            // 계산된 각도로 회전값을 만듭니다.
            Quaternion rotation = Quaternion.Euler(Angles.y, Angles.x, 0); // 참고: 원본 코드를 보니 Angles.x가 pitch, Angles.y가 yaw일 수 있습니다. 순서 확인 필요.
                                                                           // 일반적인 구현이라면 Quaternion.Euler(Angles.x, Angles.y, 0)가 맞습니다.

            // 플레이어 위치와 오프셋, 거리를 기준으로 카메라가 있어야 할 이상적인 위치를 계산합니다.
            Vector3 lookPoint = m_Player.position + m_LookAtOffset;
            Vector3 desiredPosition = lookPoint - (rotation * Vector3.forward * TargetDistance);

            // 3. 계산된 위치에 대해 충돌 처리를 적용하여 최종 위치를 결정합니다.
            PositionCameraWithCollision(desiredPosition, lookPoint);

            // 4. 카메라 타겟 오브젝트의 위치를 갱신합니다.
            UpdateTargetObjectPosition();
        }
    }
}