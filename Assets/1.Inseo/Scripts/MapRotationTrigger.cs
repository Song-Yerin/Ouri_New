using System.Collections;
using UnityEngine;

/// <summary>
/// 특정 콜라이더에 플레이어가 진입하면 맵을 회전시키고, 플레이어에게서 이펙트를 발생시킵니다.
/// </summary>
public class MapRotationTrigger : MonoBehaviour
{
    [Header("회전 대상")]
    [Tooltip("회전시킬 대상이 되는 Map 오브젝트를 여기에 할당하세요.")]
    public Transform mapObject;

    [Header("회전 방식")]
    [Tooltip("회전할 축을 선택하세요.")]
    public RotationAxis axisToRotate = RotationAxis.Y;
    public enum RotationAxis { X, Y, Z }

    [Tooltip("회전할 각도를 입력하세요. (예: 180, 90)")]
    public float rotationAngle = 180f;

    [Tooltip("맵이 회전하는 데 걸리는 시간 (초)")]
    public float rotationDuration = 2.0f;

    // --- [핵심 추가] ---
    [Header("플레이어 효과")]
    [Tooltip("맵 회전 시 플레이어 위치에 생성될 이펙트 프리팹")]
    public GameObject playerEffectPrefab;
    // --------------------

    [Header("트리거 설정")]
    [Tooltip("한 번만 작동하게 하려면 체크하세요.")]
    public bool triggerOnce = true;

    private bool isRotating = false;
    private bool hasTriggered = false;

    private void Awake()
    {
        // ... (기존과 동일)
        Collider col = GetComponent<Collider>();
        if (col == null) { col = gameObject.AddComponent<BoxCollider>(); }
        col.isTrigger = true;
        if (mapObject == null) { this.enabled = false; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isRotating || (triggerOnce && hasTriggered))
        {
            return;
        }

        // --- [핵심 추가] ---
        // 이펙트 프리팹이 할당되어 있다면, 플레이어의 위치에 생성합니다.
        if (playerEffectPrefab != null)
        {
            // 이펙트를 플레이어의 발밑 중앙 정도에 생성합니다.
            Vector3 playerPosition = other.transform.position;
            GameObject effectInstance = Instantiate(playerEffectPrefab, other.transform);
            effectInstance.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        }
        // --------------------

        // 맵 회전 시작
        StartCoroutine(RotateMapSmoothly());
    }

    private IEnumerator RotateMapSmoothly()
    {
        // ... (기존 코루틴 내용은 그대로)
        isRotating = true;
        hasTriggered = true;

        Quaternion startRotation = mapObject.rotation;
        Vector3 rotationVector = Vector3.zero;

        switch (axisToRotate)
        {
            case RotationAxis.X: rotationVector = new Vector3(rotationAngle, 0, 0); break;
            case RotationAxis.Y: rotationVector = new Vector3(0, rotationAngle, 0); break;
            case RotationAxis.Z: rotationVector = new Vector3(0, 0, rotationAngle); break;
        }

        Quaternion targetRotation = startRotation * Quaternion.Euler(rotationVector);
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            mapObject.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mapObject.rotation = targetRotation;
        isRotating = false;
        Debug.Log("맵 회전 완료!");
    }
}
