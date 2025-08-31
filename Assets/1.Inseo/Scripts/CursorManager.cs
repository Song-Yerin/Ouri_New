// CursorManager.cs (기존 코드 교체)
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void OnEnable()
    {
        // 현재 상태 즉시 반영
        Apply(UIState.CursorShown);
        // 상태 변화 구독
        UIState.OnCursorChanged += Apply;
    }

    void OnDisable()
    {
        UIState.OnCursorChanged -= Apply;
    }

    void Start()
    {
        // 기본은 숨김(게임플레이 상태)
        if (!UIState.CursorShown)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        // ESC로 임시 토글 (원하면 유지, 아니면 삭제)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIState.CursorShown) UIState.PopCursor();  // 커서 보이는 중 → 숨김
            else UIState.PushCursor(); // 커서 숨김 중 → 보임
        }

       
    }

    void Apply(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
