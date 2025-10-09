using UnityEngine;

public class SkyboxRotate : MonoBehaviour
{
    [Header("도/초 단위 속도")]
    public float rotationSpeed = 20f; // 초당 0.5도 → 아주 천천히 돎

    private float currentAngle = 0f;

    void Update()
    {
        currentAngle += rotationSpeed * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", currentAngle);
    }
}

