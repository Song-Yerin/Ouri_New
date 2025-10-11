using UnityEngine;
using System.Collections;
using System; // Action 사용을 위해 추가

public class RestartZone : MonoBehaviour
{
    [Header("필수 설정")]
    [Tooltip("플레이어가 이동될 목표 지점입니다.")]
    public Transform RestartPlace;

    [Tooltip("사망 연출을 담당하는 DeathFaceEffect 스크립트가 있는 UI 오브젝트를 연결합니다.")]
    public DeathFaceEffect deathEffect;

    // 중복 실행을 방지하기 위한 플래그
    private bool isRestarting = false;

    private void Awake()
    {
        // 이 오브젝트의 Collider를 Trigger로 설정
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError("Restart Zone 오브젝트에 Collider 컴포넌트가 없습니다!", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isRestarting)
        {
            if (RestartPlace == null || deathEffect == null)
            {
                Debug.LogError("RestartPlace 또는 DeathEffect가 설정되지 않았습니다!", this);
                return;
            }

            isRestarting = true;

            // --- [핵심 로직] ---
            // 1. 플레이어 이동 로직을 'Action' 변수에 담습니다.
            Action restartPlayerAction = () =>
            {
                CharacterController characterController = other.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                    other.transform.position = RestartPlace.position;
                    other.transform.rotation = RestartPlace.rotation;
                    characterController.enabled = true;
                }
                else
                {
                    other.transform.position = RestartPlace.position;
                    other.transform.rotation = RestartPlace.rotation;
                }
                Debug.Log(other.name + "이(가) 재시작 위치로 이동했습니다.");
            };

            // 2. DeathFaceEffect의 연출 함수를 호출하면서, 위에서 만든 '플레이어 이동' 작업을 전달합니다.
            deathEffect.PlayDeathSequence(restartPlayerAction);

            // 3. 연출이 모두 끝난 후 중복 방지 플래그를 해제하기 위한 코루틴을 실행합니다.
            StartCoroutine(WaitForRestartSequenceToEnd());
        }
    }

    private IEnumerator WaitForRestartSequenceToEnd()
    {
        // DeathFaceEffect의 연출이 시작되기를 기다렸다가,
        yield return new WaitUntil(() => deathEffect.IsPlaying);
        // 다시 연출이 끝나기를 기다립니다.
        yield return new WaitUntil(() => !deathEffect.IsPlaying);

        // 모든 연출이 끝나면 다음 재시작을 위해 플래그를 해제합니다.
        isRestarting = false;
    }
}

