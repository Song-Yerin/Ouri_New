using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CutsceneEndHandler : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "NextScene"; // �̵��� �� �̸�
    public CanvasGroup fadeCanvas; // UI�� ���̵� �г� (������ �̹����� CanvasGroup �ٿ���)
    public float fadeDuration = 1f;

    [Header("YR2���� Ȱ��ȭ�� ������Ʈ��")]
    public GameObject[] objectsToActivate; // �ν����Ϳ� ���ϴ� ������Ʈ �ֱ�

    void Start()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        if (director != null)
            director.stopped += OnCutsceneEnd;

        // ���� �� ȭ�� ���
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
            StartCoroutine(FadeIn());
        }
    }

    void OnCutsceneEnd(PlayableDirector obj)
    {
        // ���� �� �̸� Ȯ��
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "YR2")
        {
            // �� �̵� ���� ������Ʈ�� Ȱ��ȭ
            foreach (var go in objectsToActivate)
            {
                if (go != null) go.SetActive(true);
            }
            Debug.Log("�� YR2 �ƾ� ���� �� ������Ʈ�� Ȱ��ȭ �Ϸ�");
        }
        else
        {
            // �⺻ ����: �� �̵�
            StartCoroutine(FadeOutAndLoadScene());
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;
    }

    IEnumerator FadeOutAndLoadScene()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t / fadeDuration;
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // �� �ε�
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        // �� �� ���ڸ��� ���̵� ��
        StartCoroutine(FadeIn());
    }
}
