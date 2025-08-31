using UnityEngine;

public class KeyHintTriggerTest : MonoBehaviour
{
    public string keyName = "F";
    public string message = "��ȣ�ۿ� (F)";
    public float hintDuration = 0f; // 0�̸� ���� ���� �ʿ�

    private bool hintShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hintShown)
        {
            KeyHintSpawner.Instance.ShowWorldKeyHint(keyName, message, transform, hintDuration);
            hintShown = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hintShown)
        {
            KeyHintSpawner.Instance.RemoveHintByKey(keyName);
            hintShown = false;
        }
    }
}
