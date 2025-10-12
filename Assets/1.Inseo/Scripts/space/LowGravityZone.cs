using Controller;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LowGravityZone : MonoBehaviour
{
    [Header("중력 효과 설정")]
    [Tooltip("적용할 중력의 강도입니다. -1은 반중력, 0은 무중력, 0.5는 약한 중력입니다.")]
    public float gravityMultiplier = -1f; // <-- [핵심 추가] 중력 계수를 public으로 선언

    [Tooltip("효과가 지속될 시간(초)입니다.")]
    public float effectDuration = 5f;

    [Tooltip("이미 효과를 받고 있는 플레이어가 다시 들어왔을 때 시간을 초기화할지 여부입니다.")]
    public bool resetDurationOnReEnter = true;

    private void Awake()
    {
        // 이 오브젝트의 콜라이더를 반드시 트리거로 설정합니다.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 'Player' 태그를 가지고 있는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // 플레이어에게서 CreatureMover 컴포넌트를 가져옵니다.
            CreatureMover playerMover = other.GetComponent<CreatureMover>();

            // CreatureMover가 있다면, 중력 변경 효과를 적용합니다.
            if (playerMover != null)
            {
                Debug.Log("중력 지대 진입! " + effectDuration + "초 동안 " + gravityMultiplier + "배의 중력을 적용합니다.");

                // [핵심 수정] 하드코딩된 -1f 대신, public 변수인 gravityMultiplier를 전달합니다.
                playerMover.ApplyTemporaryGravityMultiplier(gravityMultiplier, effectDuration);
            }
        }
    }
}
