// UIState.cs (새로 추가, 아무 오브젝트에도 안 붙여도 됨)
using UnityEngine;
using System;

public static class UIState
{
    static int _cursorRequests = 0; // 커서를 보여달라는 요청 수
    public static bool CursorShown => _cursorRequests > 0;
    public static event Action<bool> OnCursorChanged;

    public static void PushCursor() { _cursorRequests++; Apply(); }
    public static void PopCursor() { _cursorRequests = Mathf.Max(0, _cursorRequests - 1); Apply(); }

    static void Apply()
    {
        bool show = _cursorRequests > 0;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
        OnCursorChanged?.Invoke(show);
    }
}
