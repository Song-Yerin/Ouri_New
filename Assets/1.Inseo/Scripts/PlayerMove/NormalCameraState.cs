using UnityEngine;
using Controller.CameraStates;

namespace Controller.CameraStates
{
    public class NormalCameraState : ICameraState
    {
        private PlayerCam m_Camera;

        // --- [제거] 고정된 오프셋 값 제거 ---
        // private float m_Offset = 1.5f; 
        // ------------------------------------

        // [제거] 불필요한 변수들 정리
        // private float m_CameraSpeed = 90f;
        // private Vector3 m_LookPoint;
        // private Vector3 m_TargetPos;

        public void OnEnter(PlayerCam camera)
        {
            m_Camera = camera;
        }

        public void OnUpdate()
        {
            if (m_Camera == null || m_Camera.Player == null) return;

            // 1. 카메라가 있고 싶어하는 위치와 바라볼 지점을 계산
            CalculateTargetPosition(out Vector3 targetPos, out Vector3 lookPoint);

            // 2. 충돌 처리를 포함한 공용 메서드를 호출
            m_Camera.PositionCameraWithCollision(targetPos, lookPoint);

            // 3. 타겟 오브젝트 위치 업데이트
            m_Camera.UpdateTargetObjectPosition();
        }

        public void OnExit()
        {
            // 상태가 변경될 때 특별히 정리할 내용이 있다면 여기에 작성
        }

        private void CalculateTargetPosition(out Vector3 targetPos, out Vector3 lookPoint)
        {
            var dir = new Vector3(0, 0, -m_Camera.Distance);
            var rot = Quaternion.Euler(m_Camera.Angles.x, m_Camera.Angles.y, 0f);

            // --- [수정] PlayerCam에 있는 LookAtOffset 값을 가져와서 사용 ---
            lookPoint = m_Camera.Player.position + m_Camera.LookAtOffset;
            // -------------------------------------------------------------
            targetPos = lookPoint + rot * dir;
        }
    }
}
