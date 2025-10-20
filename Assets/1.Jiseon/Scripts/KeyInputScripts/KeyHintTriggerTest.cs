using UnityEngine;

public class KeyHintTriggerTest : MonoBehaviour
{
    [Header("힌트 설정")]
    public string keyName = "F";              // 비워두면 텍스트만 중앙 표시
    public string message = "상호작용 (F)";
    public float hintDuration = 0f;           // 0이면 수동 제거 필요

    private bool hintShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hintShown)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                KeyHintSpawner.Instance.ShowCenterHint(message, hintDuration);
            }
            else
            {
                KeyHintSpawner.Instance.ShowWorldKeyHint(keyName, message, transform, hintDuration);
            }

            hintShown = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hintShown)
        {
            if (string.IsNullOrEmpty(keyName))
                KeyHintSpawner.Instance.RemoveCenterHint();
            else
                KeyHintSpawner.Instance.RemoveHintByKey(keyName);

            hintShown = false;
        }
    }
}
