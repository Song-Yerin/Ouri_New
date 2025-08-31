using UnityEngine;

namespace Controller.CameraStates
{
    /// ��� ī�޶� ���°� �����ؾ� �� �������̽��Դϴ�.
    public interface ICameraState
    {
        /// �� ���¿� �������� �� �� �� ȣ��˴ϴ�.
        /// <param name="camera">������ ī�޶� ��Ʈ�ѷ�</param>
        void OnEnter(PlayerCam camera);

        /// �� ���°� Ȱ��ȭ�� ���� �� ������ ȣ��˴ϴ�.
        void OnUpdate();

        /// �� ���¿��� ��� �� �� �� ȣ��˴ϴ�.
        void OnExit();
    }
}