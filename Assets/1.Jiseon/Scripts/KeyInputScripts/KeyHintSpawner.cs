using UnityEngine;
using System.Collections.Generic;

public class KeyHintSpawner : MonoBehaviour
{
    public static KeyHintSpawner Instance;

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

    private void Update()
    {
        // 테스트용: Shift 힌트 13초 동안 표시
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowKeyHint("leftShift", "대쉬하세요!", 13f);
        }

        // 테스트용: F키 힌트 (duration = 0, 수동 제거)
        if (Input.GetKeyDown(KeyCode.N))
        {
            ShowKeyHint("F", "상호작용하세요!", 0f);
        }

        // 테스트용: F키 힌트 제거 (예: 문 열기 성공 후)
        if (Input.GetKeyDown(KeyCode.M))
        {
            RemoveHintByKey("F");
        }

        // 테스트용: G키 → 타겟 오브젝트 위에 월드 힌트 표시
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameObject target = GameObject.Find("TargetObject"); // 예시용 타겟
            if (target != null)
            {
                ShowWorldKeyHint("E", "문을 여세요!", target.transform, 0f);
            }
        }
    }

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

        // 기존 힌트가 있으면 제거
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

        // 기존 월드 힌트 제거 (있다면)
        if (activeHints.TryGetValue(keyCode, out KeyHintUI existingHint))
        {
            if (existingHint != null)
                Destroy(existingHint.gameObject);
            activeHints.Remove(keyCode);
        }

        GameObject ui = Instantiate(keyHintWorldPrefab);
        ui.transform.position = target.position + new Vector3(0f, 2f, 0f); // 머리 위

        KeyHintUI hint = ui.GetComponent<KeyHintUI>();
        if (hint == null)
        {
            Debug.LogError("KeyHintUI 컴포넌트를 찾을 수 없습니다!");
            Destroy(ui);
            return;
        }

        // 키 아이콘 불러오기
        string spritePath = $"KeySprites/{keyName.ToLower()}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite == null)
            Debug.LogError($"Sprite 로드 실패: {spritePath}");

        // 힌트 초기화
        hint.Init(sprite, message, duration);
        hint.watchKey = keyCode;

        if (duration <= 0f)
            activeHints[keyCode] = hint;

        // 타겟 따라가게 설정
        FollowTargetUI follow = ui.GetComponent<FollowTargetUI>();
        if (follow == null)
            follow = ui.AddComponent<FollowTargetUI>();

        follow.target = target;

        // 카메라 바라보도록
        if (ui.GetComponent<LookAtCamera>() == null)
            ui.AddComponent<LookAtCamera>();
    }



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
}
