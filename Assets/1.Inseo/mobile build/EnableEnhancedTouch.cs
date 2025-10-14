using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class EnableEnhancedTouch : MonoBehaviour
{
    // 씬이 로드될 때 자동으로 호출됩니다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // Enhanced Touch API를 활성화합니다.
        EnhancedTouchSupport.Enable();
        Debug.Log("Enhanced Touch Support Enabled.");
    }
}
