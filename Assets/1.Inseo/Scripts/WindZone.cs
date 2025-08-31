using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(Collider))]
    public class WindZone : MonoBehaviour
    {
        [Header("Wind Zone Settings")]
        [Tooltip("플레이어를 위로 밀어 올리는 힘의 세기입니다.")]
        public float power = 10f;

        private Collider zoneCollider;

        // 영역 안에 있는 모든 CreatureMover를 추적하기 위해 리스트를 사용합니다.
        // 여러 명이 동시에 들어올 경우도 대비할 수 있습니다.
        private System.Collections.Generic.List<CreatureMover> playersInZone = new System.Collections.Generic.List<CreatureMover>();

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            CreatureMover mover = other.GetComponent<CreatureMover>();
            if (mover != null && !playersInZone.Contains(mover))
            {
                playersInZone.Add(mover);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            CreatureMover mover = other.GetComponent<CreatureMover>();
            if (mover != null)
            {
                playersInZone.Remove(mover);
            }
        }

        // 물리 효과는 FixedUpdate에서 처리합니다.
        private void FixedUpdate()
        {
            // 리스트에 있는 모든 플레이어를 순회합니다.
            foreach (CreatureMover player in playersInZone)
            {
                // IsGliding 프로퍼티는 CreatureMover에 있어야 합니다.
                // 만약 없다면, player.GetComponent<CreatureMover>().IsGliding 처럼 접근해야 합니다.
                // 이 예제에서는 CreatureMover에 public bool IsGliding { get; } 이 있다고 가정합니다.

                // 플레이어가 활공 상태일 때만 힘을 적용합니다.
                if (player != null && player.IsGliding)
                {
                    // Time.fixedDeltaTime을 곱하여 프레임 속도에 관계없이 일정한 힘을 주도록 합니다.
                    Vector3 windDirection = transform.up;
                    Vector3 windForce = windDirection * power * Time.fixedDeltaTime;

                    // 플레이어의 CharacterController를 직접 찾아 Move 메서드를 호출합니다.
                    CharacterController controller = player.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.Move(windForce);
                    }
                }
            }
        }
    }
}
