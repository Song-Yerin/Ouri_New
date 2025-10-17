using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    public class PlayerInputforMobile : MonoBehaviour
    {
        // ... (기존 참조 설정 및 변수들 유지) ...
        [Header("참조 설정")]
        [SerializeField] private PlayerCam m_Camera;
        [SerializeField] private SimpleJoystick m_Joystick;

        [Header("카메라 설정")]
        [SerializeField] private float m_CameraLookSpeed = 1.0f;

        // --- [새 변수 추가] ---
        [Header("부드러운 회전 설정")]
        [Tooltip("회전 감속 속도. 값이 클수록 빨리 멈춥니다.")]
        [SerializeField] private float m_RotationDamping = 10.0f;

        private Vector2 m_RotationInput; // 현재 프레임의 회전 입력 값을 저장할 변수
        // -----------------------

        private CreatureMover m_Mover;
        private bool m_IsRun = false;
        private bool isDragging = false;
        private bool isTouchDownOnUI = false;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }

        private void Update()
        {
            // 1. 조이스틱 이동 입력
            Vector2 axisInput;
            if (m_Joystick != null && m_Joystick.InputDirection.sqrMagnitude > 0)
            {
                axisInput = m_Joystick.InputDirection;
            }
            else
            {
                axisInput = Vector2.zero;
            }

            // 2. 드래그 감지 및 카메라 회전 입력 처리
            HandleDragInput();

            // 3. CreatureMover에 이동/달리기 값 전달
            Vector3 target = (m_Camera == null) ? Vector3.zero : m_Camera.Target.position;
            m_Mover.SetInput(axisInput, target, m_IsRun, Vector2.zero, 0f);
        }

        /// <summary>
        /// 드래그 입력을 직접 처리하고, 부드러운 감속 로직을 포함하는 함수
        /// </summary>
        private void HandleDragInput()
        {
            // --- 포인터 다운 감지 ---
            if (Input.GetMouseButtonDown(0))
            {
                isTouchDownOnUI = IsPointerOverUI();
            }

            // --- 포인터 누르고 있는 중 감지 ---
            if (Input.GetMouseButton(0))
            {
                if (!isTouchDownOnUI)
                {
                    Vector2 dragVector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    if (dragVector.sqrMagnitude > 0)
                    {
                        isDragging = true;
                        // 입력 값을 즉시 적용하지 않고 m_RotationInput에 누적
                        m_RotationInput = dragVector * m_CameraLookSpeed;
                    }
                }
            }

            // --- 포인터 뗄 때 감지 ---
            if (Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    isDragging = false;
                }
                isTouchDownOnUI = false;
            }

            // --- [핵심 수정] 항상 카메라에 회전 값 전달 및 감속 처리 ---
            if (m_Camera != null && m_RotationInput.sqrMagnitude > 0.001f)
            {
                // 계산된 회전 값을 카메라로 전달
                m_Camera.UpdateRotation(m_RotationInput);

                // 전달한 값을 점차 0으로 줄여서 회전이 서서히 멈추게 함
                m_RotationInput = Vector2.Lerp(m_RotationInput, Vector2.zero, Time.deltaTime * m_RotationDamping);
            }
            else
            {
                m_RotationInput = Vector2.zero;
            }
        }

        private bool IsPointerOverUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        // --- UI 버튼 이벤트 함수들 ---
        public void OnRunToggleChanged(bool isOn)
        {
            m_IsRun = isOn;
            Debug.Log("달리기 상태 변경: " + (m_IsRun ? "달리기 시작" : "걷기로 전환"));
        }

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
