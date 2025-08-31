
using UnityEngine;

public class RequiresCursor : MonoBehaviour
{
    void OnEnable() { UIState.PushCursor(); }  // UI ���� �� Ŀ�� ǥ��
    void OnDisable() { UIState.PopCursor(); }  // UI ���� �� ����
}
