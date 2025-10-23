using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PlayerFeedbackManager : MonoBehaviour
{
    [Header("생명력 설정")]
    [Tooltip("플레이어의 최대 생명력")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("피격 UI 효과")]
    [Tooltip("피격 시 화면에 표시될 할퀴기 이미지 UI")]
    public Image scratchImage;
    [Tooltip("할퀴기 이미지가 나타났다 사라지는 속도")]
    public float fadeSpeed = 2f;

    [Header("Post-Processing 충격 효과")]
    [Tooltip("효과를 적용할 Post Process Volume")]
    public Volume postProcessVolume;
    [Tooltip("효과가 지속되는 시간")]
    public float effectDuration = 0.4f;
    [Tooltip("렌즈 왜곡 강도")]
    [Range(0, 100)] public float lensMagnitude = 60f;
    [Tooltip("색수차 강도")]
    [Range(0, 1)] public float chromaticMagnitude = 1f;
    [Tooltip("피격 시 화면에 번쩍이는 붉은색")]
    public Color damageTintColor = new Color(1, 0, 0, 0.3f);

    // Post-Processing 효과 참조 변수들
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;
    private bool isShaking = false;

    void Start()
    {
        currentHealth = maxHealth;

        // 할퀴기 이미지 초기화
        if (scratchImage != null)
        {
            scratchImage.color = Color.clear;
        }

        // Post-Process Volume 및 효과 초기화
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out lensDistortion);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out colorAdjustments);
        }
        else
        {
            Debug.LogError("PlayerFeedbackManager: Post Process Volume이 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        // 피격 UI 효과 서서히 투명하게
        if (scratchImage != null && scratchImage.color.a > 0)
        {
            scratchImage.color = Color.Lerp(scratchImage.color, Color.clear, fadeSpeed * Time.deltaTime);
        }
    }

    // CatAttacker가 호출하는 유일한 함수
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"플레이어가 {damage}의 피해를 입었습니다! 남은 생명: {currentHealth}");

        // 이 스크립트 안에서 모든 효과를 직접 처리
        ShowScratchUI();
        TriggerShakeEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void CarDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"플레이어가 {damage}의 피해를 입었습니다! 남은 생명: {currentHealth}");

        TriggerShakeEffect();

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    private void ShowScratchUI()
    {
        if (scratchImage != null)
        {
            scratchImage.color = Color.white;
        }
    }

    private void TriggerShakeEffect()
    {
        if (postProcessVolume != null && !isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;

        // 코루틴 시작 시 효과 값 설정
        lensDistortion.intensity.value = -lensMagnitude;
        chromaticAberration.intensity.value = chromaticMagnitude;
        colorAdjustments.colorFilter.value = damageTintColor;

        float elapsed = 0.0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / effectDuration;

            // 서서히 원래 값으로 복원
            lensDistortion.intensity.value = Mathf.Lerp(-lensMagnitude, 0f, progress);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticMagnitude, 0f, progress);
            colorAdjustments.colorFilter.value = Color.Lerp(damageTintColor, Color.white, progress);

            yield return null;
        }

        // 확실하게 원래 값으로 복원
        lensDistortion.intensity.value = 0f;
        chromaticAberration.intensity.value = 0f;
        colorAdjustments.colorFilter.value = Color.white;

        isShaking = false;
    }

    private void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다! (게임 오버)");
        // 게임 오버 로직
    }
}
