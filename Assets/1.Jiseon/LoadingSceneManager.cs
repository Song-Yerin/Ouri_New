using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    [Header("UI 참조")]
    [SerializeField] private Slider progressSlider; // 0~1 범위
    [SerializeField] private TMP_Text progressText; // "XX %"
    [SerializeField] private TMP_Text tipText;      // 랜덤 문구 표시용 (추가)

    [Header("페이크 로딩 시간 (초)")]
    [SerializeField] private float fakeTotalLoadTime = 3f;

    [Header("로딩 중 랜덤 문구들")]
    [TextArea(2, 5)]
    [SerializeField]
    private string[] loadingTips = new string[]
    {
        "날다람쥐가 날 준비를 하고 있어요...",
        "바람의 방향을 계산 중입니다...",
        "실험용 날다람쥐가 버튼을 누르고 있어요...",
        "오늘도 맑은 하늘로 향하는 중!",
        "데이터 청소 중... 먼지가 많네요!"
    };

    private void Start()
    {
        // 랜덤 문구 표시
        if (tipText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }

        StartCoroutine(LoadSceneCoroutine());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
    }

    private IEnumerator LoadSceneCoroutine()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float startTime = Time.unscaledTime;
        bool loadDone = false;

        while (true)
        {
            yield return null;

            float fakeElapsed = Time.unscaledTime - startTime;
            float fakeProgress = Mathf.Clamp01(fakeElapsed / fakeTotalLoadTime);
            progressSlider.value = fakeProgress;
            progressText.text = $"{Mathf.RoundToInt(fakeProgress * 100f)} %";

            if (!loadDone && op.progress >= 0.9f)
            {
                loadDone = true;
            }

            if (fakeProgress >= 1f && loadDone)
            {
                op.allowSceneActivation = true;
                yield break;
            }
        }
    }
}
