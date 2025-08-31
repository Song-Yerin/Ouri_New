using UnityEngine;
namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_JumpButton = "Jump";
        [SerializeField]
        private KeyCode m_RunKey = KeyCode.LeftShift;
        [Header("Camera")]
        [SerializeField]
        private PlayerCam m_Camera;
        [SerializeField]
        private string m_MouseX = "Mouse X";
        [SerializeField]
        private string m_MouseY = "Mouse Y";
        [SerializeField]
        private string m_MouseScroll = "Mouse ScrollWheel";
        private CreatureMover m_Mover;
        private Vector2 m_Axis;
        private bool m_IsRun;
        // private bool m_IsJump; // m_IsJump는 이제 CreatureMover가 직접 관리합니다.
        private bool m_IsGlide;
        private bool m_GlideToggleRequested = false; // 이 프레임에 글라이드 토글 요청
        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;
        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }
        private void Update()
        {
            m_Axis = new Vector2(Input.GetAxis(m_HorizontalAxis), Input.GetAxis(m_VerticalAxis));
            m_IsRun = Input.GetKey(m_RunKey);

            if (Input.GetButtonDown(m_JumpButton))
            {
                if (!IsGrounded()) // 반드시 CreatureMover 통합 지상 판정 활용이 더 좋음
                    m_GlideToggleRequested = true;
                m_Mover.RequestJump();
            }

            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target.position;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);

            if (m_Mover != null)
            {
                // glide 입력을 직접 전달하지 않고(꾹 누름→유지 X), 아래처럼 토글 명령만 전달
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, in m_MouseDelta, m_Scroll);

                // 글라이드 토글 요청이 있을 때만 호출(한 프레임만)
                if (m_GlideToggleRequested)
                {
                    m_Mover.RequestGlideToggle();
                    m_GlideToggleRequested = false;
                }
            }
        }

        private bool IsGrounded()
        {
            return m_Mover != null && m_Mover.IsActuallyGrounded; // CreatureMover 공개 프로퍼티 
        }

        public void BindMover(CreatureMover mover)
        {
            m_Mover = mover;
        }
    }
}
