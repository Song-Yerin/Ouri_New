using UnityEngine;
using Controller.CameraStates;

namespace Controller.CameraStates
{
    public class NormalCameraState : ICameraState
    {
        private PlayerCam m_Camera;

        // --- [����] ������ ������ �� ���� ---
        // private float m_Offset = 1.5f; 
        // ------------------------------------

        // [����] ���ʿ��� ������ ����
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

            // 1. ī�޶� �ְ� �;��ϴ� ��ġ�� �ٶ� ������ ���
            CalculateTargetPosition(out Vector3 targetPos, out Vector3 lookPoint);

            // 2. �浹 ó���� ������ ���� �޼��带 ȣ��
            m_Camera.PositionCameraWithCollision(targetPos, lookPoint);

            // 3. Ÿ�� ������Ʈ ��ġ ������Ʈ
            m_Camera.UpdateTargetObjectPosition();
        }

        public void OnExit()
        {
            // ���°� ����� �� Ư���� ������ ������ �ִٸ� ���⿡ �ۼ�
        }

        private void CalculateTargetPosition(out Vector3 targetPos, out Vector3 lookPoint)
        {
            var dir = new Vector3(0, 0, -m_Camera.Distance);
            var rot = Quaternion.Euler(m_Camera.Angles.x, m_Camera.Angles.y, 0f);

            // --- [����] PlayerCam�� �ִ� LookAtOffset ���� �����ͼ� ��� ---
            lookPoint = m_Camera.Player.position + m_Camera.LookAtOffset;
            // -------------------------------------------------------------
            targetPos = lookPoint + rot * dir;
        }
    }
}
