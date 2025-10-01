using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;

    [Header("피격 효과 설정")]
    public Image scratchImage;
    public float fadeSpeed = 2f;

    // --- [핵심 수정 1] --- Post-Processing 효과 강도 설정 변수 추가
    [Header("Post-Processing 충격 효과")]
    [Tooltip("효과가 지속되는 시간")]
    public float effectDuration = 0.4f;
    [Tooltip("렌즈 왜곡 강도")]
    public float lensMagnitude = 60f;
    [Tooltip("색수차 강도")]
    public float chromaticMagnitude = 1f;


    private int currentHealth;

    // (Start, Update 함수는 기존과 동일)
    void Start()
    {
        currentHealth = maxHealth;
        if (scratchImage != null) scratchImage.color = Color.clear;
    }

    void Update()
    {
        if (scratchImage != null && scratchImage.color.a > 0)
        {
            Color newColor = scratchImage.color;
            newColor.a -= fadeSpeed * Time.deltaTime;
            scratchImage.color = newColor;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"플레이어가 {damage}의 피해를 입었습니다! 남은 생명: {currentHealth}");

        ShowScratchEffect();

        // --- [핵심 수정 2] --- PostProcessingShaker 함수 호출
        if (PostProcessingShaker.Instance != null)
        {
          //  PostProcessingShaker.Instance.StartShake(effectDuration, lensMagnitude, chromaticMagnitude);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowScratchEffect()
    {
        if (scratchImage != null)
        {
            scratchImage.color = Color.white;
        }
    }

    private void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다! (게임 오버)");
    }
}
