using UnityEngine;

[System.Serializable]
public struct LookUpPreset
{
    [Tooltip("Framing Transposer의 Tracked Object Offset Y")]
    public float offsetY;

    [Tooltip("Composer의 ScreenY (0=아래, 1=위)")]
    [Range(0f, 1f)] public float screenY;

    [Header("FOV를 절대값으로 지정할지")]
    public bool overrideFov;

    [Tooltip("overrideFov가 true일 때 적용될 절대 FOV 값")]
    [Range(10f, 100f)] public float fov;
}