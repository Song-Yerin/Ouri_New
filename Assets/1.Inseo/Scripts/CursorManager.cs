// CursorManager.cs (���� �ڵ� ��ü)
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void OnEnable()
    {
        // ���� ���� ��� �ݿ�
        Apply(UIState.CursorShown);
        // ���� ��ȭ ����
        UIState.OnCursorChanged += Apply;
    }

    void OnDisable()
    {
        UIState.OnCursorChanged -= Apply;
    }

    void Start()
    {
        // �⺻�� ����(�����÷��� ����)
        if (!UIState.CursorShown)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        // ESC�� �ӽ� ��� (���ϸ� ����, �ƴϸ� ����)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIState.CursorShown) UIState.PopCursor();  // Ŀ�� ���̴� �� �� ����
            else UIState.PushCursor(); // Ŀ�� ���� �� �� ����
        }

       
    }

    void Apply(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
