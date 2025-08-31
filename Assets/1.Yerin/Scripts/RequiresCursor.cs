
using UnityEngine;

public class RequiresCursor : MonoBehaviour
{
    void OnEnable() { UIState.PushCursor(); }  // UI 켜질 때 커서 표시
    void OnDisable() { UIState.PopCursor(); }  // UI 꺼질 때 원복
}
