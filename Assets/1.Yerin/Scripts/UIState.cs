// UIState.cs (���� �߰�, �ƹ� ������Ʈ���� �� �ٿ��� ��)
using UnityEngine;
using System;

public static class UIState
{
    static int _cursorRequests = 0; // Ŀ���� �����޶�� ��û ��
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
