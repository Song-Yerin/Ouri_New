using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class StartChasingModule : MonoBehaviour
{
    [Header("참조 설정")]
    [Tooltip("추격을 시작할 Enemy 오브젝트")]
    public GameObject enemy;
    [Tooltip("추격 대상인 Player 오브젝트")]
    public Transform player;
    [Tooltip("재생할 컷씬 비디오 플레이어")]
    public VideoPlayer cutsceneVideoPlayer;

    [Header("리셋 위치")]
    public Transform playerResetPosition;
    public Transform enemyResetPosition;

    // 내부 변수
    private SmartChaseAI smartChaseAI;
    private bool hasTriggered = false;

    void Start()
    {
        if (enemy == null)
        {
            Debug.LogError("Enemy가 할당되지 않았습니다!");
            this.enabled = false;
            return;
        }

        // Enemy에서 SmartChaseAI 컴포넌트를 가져옵니다.
        smartChaseAI = enemy.GetComponent<SmartChaseAI>();

        if (smartChaseAI == null)
        {
            Debug.LogError("Enemy 오브젝트에 SmartChaseAI 스크립트가 없습니다!");
            this.enabled = false;
            return;
        }

        // 시작 시에는 AI를 비활성화하여 멋대로 움직이지 않게 합니다.
        smartChaseAI.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 감지하고, 한 번만 실행되도록 합니다.
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(StartCutsceneAndChase());
    }

    /// <summary>
    /// 컷씬을 재생하고, 끝난 뒤 추격을 시작하는 전체 시퀀스입니다.
    /// </summary>
    private IEnumerator StartCutsceneAndChase()
    {
        // 컷씬 재생
        if (cutsceneVideoPlayer != null)
        {
            cutsceneVideoPlayer.enabled = true;
            cutsceneVideoPlayer.Play();
            // 영상이 끝날 때까지 대기
            yield return new WaitUntil(() => !cutsceneVideoPlayer.isPlaying && cutsceneVideoPlayer.time > 0);
            cutsceneVideoPlayer.Stop();
            cutsceneVideoPlayer.enabled = false;
        }

        // 컷씬이 끝난 후, AI의 추격을 시작시킵니다.
        StartEnemyChase();
    }

    /// <summary>
    /// SmartChaseAI 스크립트를 활성화하여 추격을 시작합니다.
    /// </summary>
    private void StartEnemyChase()
    {
        if (smartChaseAI != null)
        {
            Debug.Log("추격 시작!");
            smartChaseAI.enabled = true;
        }
    }

    /// <summary>
    /// 외부에서 호출하여 추격을 성공적으로 중지시킵니다. (예: 도착 지점 도달 시)
    /// </summary>
    public void StopChaseOnSuccess()
    {
        if (smartChaseAI != null)
        {
            smartChaseAI.enabled = false;
        }
    }

    /// <summary>
    /// 외부에서 호출하여 플레이어와 적을 리셋하는 시퀀스를 시작합니다. (예: 플레이어 사망 시)
    /// </summary>
    public void TriggerResetSequence()
    {
        StartCoroutine(ResetRoutine());
    }

    /// <summary>
    /// 플레이어와 적을 리셋하고, 2초 후 추격을 재개하는 루틴입니다.
    /// </summary>
    private IEnumerator ResetRoutine()
    {
        // 1. 추격 즉시 중지
        if (smartChaseAI != null)
        {
            smartChaseAI.enabled = false;
        }

        // 2. 플레이어와 적의 위치 및 상태 리셋
        player.position = playerResetPosition.position;
        // 플레이어도 CharacterController를 쓴다면 비활성화/활성화 처리가 필요할 수 있습니다.

        if (smartChaseAI != null)
        {
            smartChaseAI.ResetAIAndTeleport(enemyResetPosition.position, enemyResetPosition.rotation);
        }

        // 3. 2초 대기
        yield return new WaitForSeconds(2f);

        // 4. 추격 재개
        StartEnemyChase();
    }
}
