using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 시스템 사용을 위해 필수

public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI 참조")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    [Header("조이스틱 설정")]
    [Tooltip("핸들이 움직일 수 있는 최대 반경(px)")]
    public float joystickRadius = 100f;

    // 외부에서 읽을 최종 입력 값 (-1 ~ 1)
    public Vector2 InputDirection { get; private set; }
    [SerializeField] private Vector2 debug_InputDirection;

    private Vector2 startPosition;

    private void Start()
    {
        // 시작 시 핸들을 중앙에 위치
        joystickHandle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }

    // 터치(또는 마우스 클릭)가 시작될 때 1번 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        // 현재 터치 위치를 기준으로 드래그 시작점을 계산
        startPosition = eventData.position;
        OnDrag(eventData); // 시작 시에도 드래그를 한 번 호출하여 즉시 반응
    }

    // 터치(또는 마우스 클릭) 상태로 드래그하는 동안 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 moveVector = eventData.position - startPosition;
        Vector2 clampedVector = Vector2.ClampMagnitude(moveVector, joystickRadius);

        // 핸들 위치 업데이트
        joystickHandle.anchoredPosition = clampedVector;
        debug_InputDirection = InputDirection;

        // 실제 플레이어가 사용할 -1 ~ 1 사이의 값으로 정규화
        InputDirection = clampedVector / joystickRadius;
    }

    // 터치(또는 마우스 클릭)를 뗄 때 1번 호출
    public void OnPointerUp(PointerEventData eventData)
    {
        // 핸들과 입력 값을 모두 초기화
        joystickHandle.anchoredPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }
}
