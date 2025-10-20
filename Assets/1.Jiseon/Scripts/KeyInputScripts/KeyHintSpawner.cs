using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KeyHintSpawner : MonoBehaviour
{
    public static KeyHintSpawner Instance;

    [Header("UI 프리팹")]
    public GameObject keyHintPrefab;         // 화면 UI용 (Screen Space)
    public GameObject keyHintWorldPrefab;    // 월드 UI용 (World Space)

    private Dictionary<KeyCode, KeyHintUI> activeHints = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ======================
    // 화면 UI용 (기존 기능)
    // ======================
    public void ShowKeyHint(string keyName, string message, float duration = 3f)
    {
        if (keyHintPrefab == null)
        {
            Debug.LogError("KeyHint 프리팹이 연결되지 않았습니다!");
            return;
        }

        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'은 KeyCode로 인식되지 않습니다.");
            return;
        }

        if (activeHints.TryGetValue(keyCode, out KeyHintUI existingHint))
        {
            if (existingHint != null)
                Destroy(existingHint.gameObject);
            activeHints.Remove(keyCode);
        }

        GameObject ui = Instantiate(keyHintPrefab, GameObject.Find("Canvas").transform);
        KeyHintUI hint = ui.GetComponent<KeyHintUI>();

        string spritePath = $"KeySprites/{keyName.ToLower()}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);

        if (sprite == null)
            Debug.LogError($"Sprite 로드 실패: {spritePath}");

        hint.Init(sprite, message, duration);
        hint.watchKey = keyCode;

        if (duration <= 0f)
            activeHints[keyCode] = hint;
    }

    // ======================
    // 월드 UI용 (기존 기능)
    // ======================
    public void ShowWorldKeyHint(string keyName, string message, Transform target, float duration = 3f)
    {
        if (keyHintWorldPrefab == null)
        {
            Debug.LogError("월드 힌트 프리팹이 설정되지 않았습니다!");
            return;
        }

        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'은 KeyCode로 인식되지 않습니다.");
            return;
        }

        if (activeHints.TryGetValue(keyCode, out KeyHintUI existingHint))
        {
            if (existingHint != null)
                Destroy(existingHint.gameObject);
            activeHints.Remove(keyCode);
        }

        GameObject ui = Instantiate(keyHintWorldPrefab);
        ui.transform.position = target.position + new Vector3(0f, 2f, 0f);

        KeyHintUI hint = ui.GetComponent<KeyHintUI>();
        if (hint == null)
        {
            Debug.LogError("KeyHintUI 컴포넌트를 찾을 수 없습니다!");
            Destroy(ui);
            return;
        }

        string spritePath = $"KeySprites/{keyName.ToLower()}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite == null)
            Debug.LogError($"Sprite 로드 실패: {spritePath}");

        hint.Init(sprite, message, duration);
        hint.watchKey = keyCode;

        if (duration <= 0f)
            activeHints[keyCode] = hint;

        FollowTargetUI follow = ui.GetComponent<FollowTargetUI>();
        if (follow == null)
            follow = ui.AddComponent<FollowTargetUI>();

        follow.target = target;

        if (ui.GetComponent<LookAtCamera>() == null)
            ui.AddComponent<LookAtCamera>();
    }

    // ======================
    // 키 이름으로 힌트 제거
    // ======================
    public void RemoveHintByKey(string keyName)
    {
        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'은 KeyCode로 인식되지 않습니다.");
            return;
        }

        if (activeHints.TryGetValue(keyCode, out KeyHintUI hint))
        {
            if (hint != null)
            {
                hint.NotifyMissionComplete();
                activeHints.Remove(keyCode);
            }
        }
    }

    // ======================
    // 중앙 텍스트만 표시 (프리팹 재활용)
    // ======================
    public void ShowCenterHint(string message, float duration = 3f)
    {
        if (keyHintPrefab == null)
        {
            Debug.LogError("KeyHint 프리팹이 연결되지 않았습니다!");
            return;
        }

        GameObject ui = Instantiate(keyHintPrefab, GameObject.Find("Canvas").transform);
        KeyHintUI hint = ui.GetComponent<KeyHintUI>();

        // 아이콘 비활성화
        if (hint.keyImage != null)
            hint.keyImage.gameObject.SetActive(false);

        // 텍스트만 표시
        if (hint.description != null)
            hint.description.text = message;

        // 가로 중앙, 세로 기존 유지
        RectTransform rect = ui.GetComponent<RectTransform>();
        if (rect != null)
            rect.anchoredPosition = new Vector2(0, rect.anchoredPosition.y);

        // 코루틴 실행
        hint.StopAllCoroutines();
        StartCoroutine(FadeInCenterHint(hint, duration));
    }

    private IEnumerator FadeInCenterHint(KeyHintUI hint, float duration)
    {
        // 완전 투명에서 시작
        var setAlphaMethod = hint.GetType().GetMethod("SetAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        setAlphaMethod.Invoke(hint, new object[] { 0f });

        // hint의 FadeIn() 실행 (코루틴 직접 참조)
        IEnumerator fadeIn = (IEnumerator)hint
            .GetType()
            .GetMethod("FadeIn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(hint, null);

        yield return StartCoroutine(fadeIn);

        // 유지 후 FadeOut
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
            hint.NotifyMissionComplete();
        }
    }





    public void RemoveCenterHint()
    {
        foreach (Transform child in GameObject.Find("Canvas").transform)
        {
            if (child.name.Contains("KeyHint"))
                Destroy(child.gameObject);
        }
    }
}
