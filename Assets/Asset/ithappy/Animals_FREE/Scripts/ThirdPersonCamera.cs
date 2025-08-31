using UnityEngine;

namespace Controller
{
    public class ThirdPersonCamera : PlayerCamera
    {
        [Header("�Ϲ� ����")]
        [SerializeField, Range(0f, 2f)]
        private float m_Offset = 1.5f;
        [SerializeField, Range(0f, 360f)]
        private float m_CameraSpeed = 90f;

        // [�����̵� ��� �߰�] �����̵� �� ī�޶� ������ ���� ������
        [Header("�����̵� ����")]
        [SerializeField]
        private float m_SlideDistance = 5.0f; // �÷��̾�κ��� ������ �Ÿ�
        [SerializeField]
        private float m_SlideHeight = 2.0f; // �÷��̾�� ���� ����
        [SerializeField]
        private float m_SlideCameraSpeed = 180f; // �����̵� ����� ���� ī�޶� ��ȯ �ӵ�


        private Vector3 m_LookPoint;
        private Vector3 m_TargetPos;

        // [�����̵� ��� �߰�] ī�޶��� ���� ���¸� ������ ����
        private bool m_IsSliding = false;
        private Vector3 m_SlideDirection = Vector3.forward;

        // [SOLID ��Ģ �����丵] LateUpdate�� ���¿� ���� � ī�޶� ������ ���������� �����մϴ�.
        private void LateUpdate()
        {
            if (m_IsSliding)
            {
                UpdateSlideCamera(Time.deltaTime);
            }
            else
            {
                UpdateNormalCamera(Time.deltaTime);
            }

            // Ÿ�� ������Ʈ�� ī�޶� ���¿� �����ϹǷ� �׻� �������� ȣ���մϴ�.
            UpdateTargetPosition();
        }

        public override void SetInput(in Vector2 delta, float scroll)
        {
            base.SetInput(delta, scroll);

            // [�����̵� ��� �߰�] �����̵� �߿��� ���콺 �Է����� ī�޶� ȸ������ �ʵ��� �մϴ�.
            if (m_IsSliding) return;

            CalculateNormalTargetPosition();
        }

        /// <summary>
        /// [�����̵� ��� �߰�] CreatureMover���� ī�޶��� ���¸� �����ϱ� ���� ȣ���� �޼����Դϴ�.
        /// </summary>
        public void SetSlideState(bool isSliding, Vector3 slideDirection)
        {
            m_IsSliding = isSliding;
            // �̲������� ������ ���� ��ǥ �������� �����մϴ�.
            m_SlideDirection = slideDirection;
        }

        /// <summary>
        /// [SOLID ��Ģ �����丵] �Ϲ� ������ ���� ī�޶� �����Դϴ�.
        /// </summary>
        private void UpdateNormalCamera(float deltaTime)
        {
            MoveCamera(deltaTime, m_CameraSpeed);
            m_Transform.LookAt(m_LookPoint);
        }

        /// <summary>
        /// [�����̵� ��� �߰�] �����̵� ������ ���� ī�޶� �����Դϴ�.
        /// </summary>
        private void UpdateSlideCamera(float deltaTime)
        {
            // ��ǥ: �÷��̾� ����, �̲������� ������ �ݴ���
            var playerPos = (m_Player == null) ? Vector3.zero : m_Player.position;
            Vector3 desiredPosition = playerPos - m_SlideDirection * m_SlideDistance + Vector3.up * m_SlideHeight;

            m_LookPoint = playerPos + m_Offset * Vector3.up;
            m_TargetPos = desiredPosition;

            MoveCamera(deltaTime, m_SlideCameraSpeed);
            m_Transform.LookAt(m_LookPoint);
        }

        /// <summary>
        /// [SOLID ��Ģ �����丵] Move()���� �и��� ���� ī�޶� �̵� �޼����Դϴ�.
        /// </summary>
        private void MoveCamera(float deltaTime, float speed)
        {
            var direction = m_TargetPos - m_Transform.position;
            var delta = speed * deltaTime;

            // �ε巯�� �̵� (Lerp�� ���������� �����ӿ� ������)
            if (delta * delta > direction.sqrMagnitude)
            {
                m_Transform.position = m_TargetPos;
            }
            else
            {
                m_Transform.position += delta * direction.normalized;
            }
        }

        /// <summary>
        /// [SOLID ��Ģ �����丵] �Ϲ� ������ ���� ��ǥ ��ġ ��� �����Դϴ�.
        /// </summary>
        private void CalculateNormalTargetPosition()
        {
            //�������� �ּ�ó����. �Ⱦ�����.
            //var dir = new Vector3(0, 0, -m_Distance);
            //var rot = Quaternion.Euler(m_Angles.x, m_Angles.y, 0f);

            var playerPos = (m_Player == null) ? Vector3.zero : m_Player.position;
            m_LookPoint = playerPos + m_Offset * Vector3.up;
            //m_TargetPos = m_LookPoint + rot * dir;
        }

        /// <summary>
        /// [SOLID ��Ģ �����丵] Move()���� �и��� Ÿ�� ��ġ ������Ʈ �޼����Դϴ�.
        /// </summary>
        private void UpdateTargetPosition()
        {
            if (m_Target == null)
            {
                return;
            }
            m_Target.position = m_Transform.position + m_Transform.forward * TargetDistance;
        }
    }
}
