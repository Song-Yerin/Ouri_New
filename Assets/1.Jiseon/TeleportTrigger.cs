using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportTrigger : MonoBehaviour
{
    [Header("�ڷ���Ʈ ����")]
    public string targetSceneName;   // �̵��� �� �̸�
    public float delay = 1f;         // �� �� �Ŀ� �� ��ȯ����

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Ʈ���� ����: {other.name}");   // �� �̰� �� �߰�
        // �÷��̾� ���� (�±׷� ����)
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
