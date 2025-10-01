using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    // 이 스크립트의 유일한 인스턴스를 저장하는 static 변수
    public static CameraShaker Instance { get; private set; }

    private Vector3 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        // 싱글톤 패턴: 씬에 CameraShaker가 하나만 존재하도록 보장
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 카메라의 원래 위치를 저장
        originalPosition = transform.localPosition;
    }

    // 화면을 흔드는 코루틴을 시작시키는 공개 함수
    public void StartShake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(Shake(duration, magnitude));
        }
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 원래 위치 주변의 랜덤한 지점으로 카메라를 이동
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 흔들림이 끝나면 카메라를 원래 위치로 복원
        transform.localPosition = originalPosition;
        isShaking = false;
    }
}
