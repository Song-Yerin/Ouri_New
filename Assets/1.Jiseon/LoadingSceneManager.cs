using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    [Header("UI ����")]
    [SerializeField] private Slider progressSlider; // 0~1 ����
    [SerializeField] private TMP_Text progressText; // "XX %"

    [Header("����ũ �ε� �ð� (��)")]
    [SerializeField] private float fakeTotalLoadTime = 3f;

    private void Start()
    {
        StartCoroutine(LoadSceneCoroutine());
    }

    /// <summary>
    /// �ܺο��� ȣ�� �� Loading ������ �Ѿ����
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
    }

    private IEnumerator LoadSceneCoroutine()
    {
        yield return null; // �� ������ ���

        // ���� �񵿱� �ε� ����
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float startTime = Time.unscaledTime;
        bool loadDone = false;

        while (true)
        {
            yield return null;

            // 1) ����ũ Ÿ�̸� ��� �����̴�
            float fakeElapsed = Time.unscaledTime - startTime;
            float fakeProgress = Mathf.Clamp01(fakeElapsed / fakeTotalLoadTime);
            progressSlider.value = fakeProgress;
            progressText.text = $"{Mathf.RoundToInt(fakeProgress * 100f)} %";

            // 2) ���� �ε� ���� üũ (op.progress�� 0~0.9 ������ �ö󰡴� 0.9 �̻��̸� �غ� �Ϸ�)
            if (!loadDone && op.progress >= 0.9f)
            {
                loadDone = true;
            }

            // 3) �� �� �������� �ٷ� �� ��ȯ
            if (fakeProgress >= 1f && loadDone)
            {
                op.allowSceneActivation = true;
                yield break;
            }
        }
    }
}
