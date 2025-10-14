using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용을 위해 추가

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    public class PlayerInputforMobile : MonoBehaviour
    {
        [Header("참조 설정")]
        [SerializeField] private PlayerCam m_Camera;
        [SerializeField] private SimpleJoystick m_Joystick; // SimpleJoystick 참조

        private CreatureMover m_Mover;
        private bool m_IsRun = false; // 달리기는 모바일에서 사용 안 함

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }

        private void Update()
        {
            Vector2 axisInput;

            // 조이스틱이 연결되어 있고, 입력이 있다면 조이스틱 값을 사용
            if (m_Joystick != null && m_Joystick.InputDirection.sqrMagnitude > 0)
            {
                axisInput = m_Joystick.InputDirection;
            }
            else // 조이스틱 입력이 없으면 키보드 값을 사용 (에디터 테스트용)
            {
                axisInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                m_IsRun = Input.GetKey(KeyCode.LeftShift);
            }

            // 점프는 키보드로 테스트
            if (Input.GetButtonDown("Jump"))
            {
                OnJump();
            }

            // CreatureMover에 최종 입력 값 전달
            Vector3 target = (m_Camera == null) ? Vector3.zero : m_Camera.Target.position;
            m_Mover.SetInput(in axisInput, in target, m_IsRun, Vector2.zero, 0f);
        }

        // 점프 버튼의 OnClick 이벤트에 이 함수를 연결
        public void OnJump()
        {
            if (m_Mover != null && !m_Mover.IsActuallyGrounded)
            {
                m_Mover.RequestGlideToggle();
            }
            else
            {
                m_Mover.RequestJump();
            }
        }
    }
}
