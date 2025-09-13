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
        // private bool m_IsJump; // m_IsJump�� ���� CreatureMover�� ���� �����մϴ�.
        private bool m_IsGlide;
        private bool m_GlideToggleRequested = false; // �� �����ӿ� �۶��̵� ��� ��û
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
                if (!IsGrounded()) // �ݵ�� CreatureMover ���� ���� ���� Ȱ���� �� ����
                    m_GlideToggleRequested = true;
                m_Mover.RequestJump();
            }

            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target.position;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);

            if (m_Mover != null)
            {
                // glide �Է��� ���� �������� �ʰ�(�� ���������� X), �Ʒ�ó�� ��� ���ɸ� ����
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, in m_MouseDelta, m_Scroll);

                // �۶��̵� ��� ��û�� ���� ���� ȣ��(�� �����Ӹ�)
                if (m_GlideToggleRequested)
                {
                    m_Mover.RequestGlideToggle();
                    m_GlideToggleRequested = false;
                }
            }
        }

        private bool IsGrounded()
        {
            return m_Mover != null && m_Mover.IsActuallyGrounded; // CreatureMover ���� ������Ƽ 
        }

        public void BindMover(CreatureMover mover)
        {
            m_Mover = mover;
        }
    }
}
