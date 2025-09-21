using System.Collections;
using UnityEngine;

/// <summary>
/// 특정 콜라이더에 플레이어가 진입하면 지정된 오브젝트를 설정된 축과 각도만큼 회전시킵니다.
/// </summary>
public class MapRotationTrigger : MonoBehaviour
{
    // 회전 축을 선택하기 위한 열거형
    public enum RotationAxis { X, Y, Z }

    [Header("회전 대상")]
    [Tooltip("회전시킬 대상이 되는 Map 오브젝트를 여기에 할당하세요.")]
    public Transform mapObject;

    [Header("회전 방식")]
    [Tooltip("회전할 축을 선택하세요.")]
    public RotationAxis axisToRotate = RotationAxis.Y;

    [Tooltip("회전할 각도를 입력하세요. (예: 180, 90)")]
    public float rotationAngle = 180f;

    [Tooltip("맵이 회전하는 데 걸리는 시간 (초)")]
    public float rotationDuration = 2.0f;

    [Header("트리거 설정")]
    [Tooltip("한 번만 작동하게 하려면 체크하세요.")]
    public bool triggerOnce = true;

    private bool isRotating = false;
    private bool hasTriggered = false;

    private void Awake()
    {
        // 스크립트가 부착된 오브젝트에 Collider가 없으면 추가하고 Trigger로 설정
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("[MapRotationTrigger] Collider가 없어 BoxCollider를 추가합니다. 영역 크기를 조절해주세요.", this.gameObject);
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;

        // mapObject가 할당되지 않았으면 경고 메시지 출력 후 스크립트 비활성화
        if (mapObject == null)
        {
            Debug.LogError("[MapRotationTrigger] 회전시킬 Map 오브젝트가 할당되지 않았습니다!", this.gameObject);
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 진입한 오브젝트가 "Player" 태그가 아니거나, 이미 회전 중이거나, 한 번만 작동하는 트리거가 이미 작동했다면 무시
        if (!other.CompareTag("Player") || isRotating || (triggerOnce && hasTriggered))
        {
            return;
        }

        // 회전 시작
        StartCoroutine(RotateMapSmoothly());
    }

    private IEnumerator RotateMapSmoothly()
    {
        isRotating = true;
        hasTriggered = true;

        Quaternion startRotation = mapObject.rotation;
        Vector3 rotationVector = Vector3.zero;

        // 선택된 축에 따라 회전 벡터 설정
        switch (axisToRotate)
        {
            case RotationAxis.X:
                rotationVector = new Vector3(rotationAngle, 0, 0);
                break;
            case RotationAxis.Y:
                rotationVector = new Vector3(0, rotationAngle, 0);
                break;
            case RotationAxis.Z:
                rotationVector = new Vector3(0, 0, rotationAngle);
                break;
        }

        // 현재 회전값에 목표 회전값을 더함
        Quaternion targetRotation = startRotation * Quaternion.Euler(rotationVector);

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            // Slerp를 사용하여 부드럽게 회전
            mapObject.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 회전이 끝난 후 정확한 목표 각도로 설정하여 오차 보정
        mapObject.rotation = targetRotation;
        isRotating = false;

        Debug.Log("맵 회전 완료!");
    }
}
