using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingShaker : MonoBehaviour
{
    public static PostProcessingShaker Instance { get; private set; }

    private Volume postProcessVolume;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    // --- [핵심 추가 1] --- ColorAdjustments 변수 추가
    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        postProcessVolume = GetComponent<Volume>();

        postProcessVolume.profile.TryGet(out lensDistortion);
        postProcessVolume.profile.TryGet(out chromaticAberration);
        // --- [핵심 추가 2] --- 프로파일에서 ColorAdjustments 효과 찾아오기
        postProcessVolume.profile.TryGet(out colorAdjustments);

        if (lensDistortion == null || chromaticAberration == null || colorAdjustments == null)
        {
            Debug.LogError("PostProcessingShaker: Profile에서 LensDistortion, ChromaticAberration, 또는 ColorAdjustments 효과를 찾을 수 없습니다.");
            this.enabled = false;
        }
    }

    // --- [핵심 수정 3] --- StartShake 함수에 Color 파라미터 추가
    public void StartShake(float duration, float lensMagnitude, float chromaticMagnitude, Color damageColor)
    {
        if (this.enabled)
        {
            StartCoroutine(Shake(duration, lensMagnitude, chromaticMagnitude, damageColor));
        }
    }

    // --- [핵심 수정 4] --- Shake 코루틴에서 Color Filter 값 조절
    private IEnumerator Shake(float duration, float lensMagnitude, float chromaticMagnitude, Color damageColor)
    {
        float elapsed = 0.0f;

        // 효과 시작 시점에 각 효과 값을 순간적으로 최대치로 설정
        lensDistortion.intensity.value = -lensMagnitude;
        chromaticAberration.intensity.value = chromaticMagnitude;
        colorAdjustments.colorFilter.value = damageColor; // 피격 색상 적용

        // 지정된 시간 동안 서서히 원래 값으로 복원
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Lerp를 이용해 부드럽게 0으로 복원
            lensDistortion.intensity.value = Mathf.Lerp(-lensMagnitude, 0f, progress);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticMagnitude, 0f, progress);
            // Color Filter 색상을 원래의 흰색으로 부드럽게 복원
            colorAdjustments.colorFilter.value = Color.Lerp(damageColor, Color.white, progress);

            yield return null;
        }

        // 확실하게 원래 값으로 복원
        lensDistortion.intensity.value = 0f;
        chromaticAberration.intensity.value = 0f;
        colorAdjustments.colorFilter.value = Color.white;
    }
}
