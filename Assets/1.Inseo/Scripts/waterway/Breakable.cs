using UnityEngine;

/// <summary>
/// 플레이어가 특정 영역에 진입하면 자식 오브젝트들을 물리적으로 분리시키는 스크립트입니다.
/// </summary>
public class Breakable : MonoBehaviour
{
    [Header("감지 영역 설정")]
    [Tooltip("플레이어의 진입을 감지할 콜라이더입니다. Is Trigger가 반드시 체크되어 있어야 합니다.")]
    public Collider detectionCollider;

    // 파괴 이벤트가 이미 발생했는지 확인하여 중복 실행을 방지하는 변수
    private bool isBroken = false;

    private void Start()
    {
        // detectionCollider가 할당되지 않았을 경우를 대비한 안전 장치
        if (detectionCollider == null)
        {
            Debug.LogError("Breakable 스크립트에 detectionCollider가 할당되지 않았습니다!", this.gameObject);
            this.enabled = false;
            return;
        }

        // detectionCollider가 트리거가 아니면 경고 메시지 출력
        if (!detectionCollider.isTrigger)
        {
            Debug.LogWarning("detectionCollider가 트리거(Is Trigger)로 설정되어 있지 않습니다. OnTriggerEnter가 호출되지 않을 수 있습니다.", this.gameObject);
        }
    }

    /// <summary>
    /// detectionCollider의 트리거 영역에 다른 콜라이더가 들어왔을 때 호출됩니다.
    /// </summary>
    /// <param name="other">진입한 다른 콜라이더</param>
    private void OnTriggerEnter(Collider other)
    {
        // 이미 부서졌거나, 들어온 오브젝트가 "Player" 태그가 아니면 무시
        if (isBroken || !other.CompareTag("Player"))
        {
            return;
        }

        // 부서짐 상태로 변경하고, 자식들에게 Rigidbody를 활성화하는 함수 호출
        isBroken = true;
        BreakApart();
    }

    /// <summary>
    /// 이 오브젝트의 모든 자식들에게 Rigidbody를 추가하고 활성화합니다.
    /// </summary>
    private void BreakApart()
    {
        // 이 게임 오브젝트 아래의 모든 자식들을 순회합니다.
        foreach (Transform child in transform)
        {
            // 이미 Rigidbody가 있는지 확인
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
            {
                // Rigidbody가 없다면 새로 추가합니다.
                rb = child.gameObject.AddComponent<Rigidbody>();
            }

            // Rigidbody의 설정을 활성화합니다.
            rb.isKinematic = false; // 물리 엔진의 제어를 받도록 설정
            rb.useGravity = true;   // 중력 사용
            rb.mass = 0.01f;

            // (선택사항) 약간의 폭발 효과를 주기 위해 힘을 가할 수도 있습니다.
            rb.AddExplosionForce(100f, transform.position, 1f);
        }

        Debug.Log(this.gameObject.name + "이(가) 파괴되었습니다.");
    }
}
