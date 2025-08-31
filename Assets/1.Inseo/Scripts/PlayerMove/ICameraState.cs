using UnityEngine;

namespace Controller.CameraStates
{
    /// 모든 카메라 상태가 구현해야 할 인터페이스입니다.
    public interface ICameraState
    {
        /// 이 상태에 진입했을 때 한 번 호출됩니다.
        /// <param name="camera">제어할 카메라 컨트롤러</param>
        void OnEnter(PlayerCam camera);

        /// 이 상태가 활성화된 동안 매 프레임 호출됩니다.
        void OnUpdate();

        /// 이 상태에서 벗어날 때 한 번 호출됩니다.
        void OnExit();
    }
}