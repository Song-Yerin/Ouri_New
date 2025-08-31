using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportTrigger : MonoBehaviour
{
    [Header("텔레포트 설정")]
    public string targetSceneName;   // 이동할 씬 이름
    public float delay = 1f;         // 몇 초 후에 씬 전환할지

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"트리거 진입: {other.name}");   // ← 이거 꼭 추가
        // 플레이어 감지 (태그로 구분)
        if (other.CompareTag("Player") && !isTeleporting)
        {
            isTeleporting = true;
            StartCoroutine(TeleportAfterDelay());
        }
    }

    IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(targetSceneName);
    }
}
