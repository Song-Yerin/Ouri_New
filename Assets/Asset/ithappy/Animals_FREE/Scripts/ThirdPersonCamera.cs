using UnityEngine;

namespace Controller
{
    public class ThirdPersonCamera : PlayerCamera
    {
        [Header("일반 설정")]
        [SerializeField, Range(0f, 2f)]
        private float m_Offset = 1.5f;
        [SerializeField, Range(0f, 360f)]
        private float m_CameraSpeed = 90f;

        // [슬라이드 기능 추가] 슬라이드 시 카메라 설정을 위한 변수들
        [Header("슬라이드 설정")]
        [SerializeField]
        private float m_SlideDistance = 5.0f; // 플레이어로부터 떨어질 거리
        [SerializeField]
        private float m_SlideHeight = 2.0f; // 플레이어보다 높을 정도
        [SerializeField]
        private float m_SlideCameraSpeed = 180f; // 슬라이드 모드일 때의 카메라 전환 속도


        private Vector3 m_LookPoint;
        private Vector3 m_TargetPos;

        // [슬라이드 기능 추가] 카메라의 현재 상태를 저장할 변수
        private bool m_IsSliding = false;
        private Vector3 m_SlideDirection = Vector3.forward;

        // [SOLID 원칙 리팩토링] LateUpdate는 상태에 따라 어떤 카메라 로직을 실행할지만 결정합니다.
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

            // 타겟 업데이트는 카메라 상태와 무관하므로 항상 마지막에 호출합니다.
            UpdateTargetPosition();
        }

        public override void SetInput(in Vector2 delta, float scroll)
        {
            base.SetInput(delta, scroll);

            // [슬라이드 기능 추가] 슬라이딩 중에는 마우스 입력으로 카메라가 회전하지 않도록 합니다.
            if (m_IsSliding) return;

            CalculateNormalTargetPosition();
        }

        /// <summary>
        /// [슬라이드 기능 추가] CreatureMover에서 카메라의 상태를 변경하기 위해 호출할 메서드입니다.
        /// </summary>
        public void SetSlideState(bool isSliding, Vector3 slideDirection)
        {
            m_IsSliding = isSliding;
            // 미끄러지는 방향을 월드 좌표 기준으로 저장합니다.
            m_SlideDirection = slideDirection;
        }

        /// <summary>
        /// [SOLID 원칙 리팩토링] 일반 상태일 때의 카메라 로직입니다.
        /// </summary>
        private void UpdateNormalCamera(float deltaTime)
        {
            MoveCamera(deltaTime, m_CameraSpeed);
            m_Transform.LookAt(m_LookPoint);
        }

        /// <summary>
        /// [슬라이드 기능 추가] 슬라이드 상태일 때의 카메라 로직입니다.
        /// </summary>
        private void UpdateSlideCamera(float deltaTime)
        {
            // 목표: 플레이어 뒤쪽, 미끄러지는 방향의 반대편
            var playerPos = (m_Player == null) ? Vector3.zero : m_Player.position;
            Vector3 desiredPosition = playerPos - m_SlideDirection * m_SlideDistance + Vector3.up * m_SlideHeight;

            m_LookPoint = playerPos + m_Offset * Vector3.up;
            m_TargetPos = desiredPosition;

            MoveCamera(deltaTime, m_SlideCameraSpeed);
            m_Transform.LookAt(m_LookPoint);
        }

        /// <summary>
        /// [SOLID 원칙 리팩토링] Move()에서 분리된 공용 카메라 이동 메서드입니다.
        /// </summary>
        private void MoveCamera(float deltaTime, float speed)
        {
            var direction = m_TargetPos - m_Transform.position;
            var delta = speed * deltaTime;

            // 부드러운 이동 (Lerp와 유사하지만 프레임에 독립적)
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
        /// [SOLID 원칙 리팩토링] 일반 상태일 때의 목표 위치 계산 로직입니다.
        /// </summary>
        private void CalculateNormalTargetPosition()
        {
            //오류나서 주석처리함. 안쓸거임.
            //var dir = new Vector3(0, 0, -m_Distance);
            //var rot = Quaternion.Euler(m_Angles.x, m_Angles.y, 0f);

            var playerPos = (m_Player == null) ? Vector3.zero : m_Player.position;
            m_LookPoint = playerPos + m_Offset * Vector3.up;
            //m_TargetPos = m_LookPoint + rot * dir;
        }

        /// <summary>
        /// [SOLID 원칙 리팩토링] Move()에서 분리된 타겟 위치 업데이트 메서드입니다.
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
