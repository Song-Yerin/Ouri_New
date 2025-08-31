using UnityEngine;
using System.Collections.Generic;

public class KeyHintSpawner : MonoBehaviour
{
    public static KeyHintSpawner Instance;

    public GameObject keyHintPrefab;         // ȭ�� UI�� (Screen Space)
    public GameObject keyHintWorldPrefab;    // ���� UI�� (World Space)

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
        // �׽�Ʈ��: Shift ��Ʈ 13�� ���� ǥ��
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowKeyHint("leftShift", "�뽬�ϼ���!", 13f);
        }

        // �׽�Ʈ��: FŰ ��Ʈ (duration = 0, ���� ����)
        if (Input.GetKeyDown(KeyCode.N))
        {
            ShowKeyHint("F", "��ȣ�ۿ��ϼ���!", 0f);
        }

        // �׽�Ʈ��: FŰ ��Ʈ ���� (��: �� ���� ���� ��)
        if (Input.GetKeyDown(KeyCode.M))
        {
            RemoveHintByKey("F");
        }

        // �׽�Ʈ��: GŰ �� Ÿ�� ������Ʈ ���� ���� ��Ʈ ǥ��
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameObject target = GameObject.Find("TargetObject"); // ���ÿ� Ÿ��
            if (target != null)
            {
                ShowWorldKeyHint("E", "���� ������!", target.transform, 0f);
            }
        }
    }

    public void ShowKeyHint(string keyName, string message, float duration = 3f)
    {
        if (keyHintPrefab == null)
        {
            Debug.LogError("KeyHint �������� ������� �ʾҽ��ϴ�!");
            return;
        }

        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'�� KeyCode�� �νĵ��� �ʽ��ϴ�.");
            return;
        }

        // ���� ��Ʈ�� ������ ����
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
            Debug.LogError($"Sprite �ε� ����: {spritePath}");

        hint.Init(sprite, message, duration);
        hint.watchKey = keyCode;

        if (duration <= 0f)
            activeHints[keyCode] = hint;
    }

    public void ShowWorldKeyHint(string keyName, string message, Transform target, float duration = 3f)
    {
        if (keyHintWorldPrefab == null)
        {
            Debug.LogError("���� ��Ʈ �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'�� KeyCode�� �νĵ��� �ʽ��ϴ�.");
            return;
        }

        // ���� ���� ��Ʈ ���� (�ִٸ�)
        if (activeHints.TryGetValue(keyCode, out KeyHintUI existingHint))
        {
            if (existingHint != null)
                Destroy(existingHint.gameObject);
            activeHints.Remove(keyCode);
        }

        GameObject ui = Instantiate(keyHintWorldPrefab);
        ui.transform.position = target.position + new Vector3(0f, 2f, 0f); // �Ӹ� ��

        KeyHintUI hint = ui.GetComponent<KeyHintUI>();
        if (hint == null)
        {
            Debug.LogError("KeyHintUI ������Ʈ�� ã�� �� �����ϴ�!");
            Destroy(ui);
            return;
        }

        // Ű ������ �ҷ�����
        string spritePath = $"KeySprites/{keyName.ToLower()}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite == null)
            Debug.LogError($"Sprite �ε� ����: {spritePath}");

        // ��Ʈ �ʱ�ȭ
        hint.Init(sprite, message, duration);
        hint.watchKey = keyCode;

        if (duration <= 0f)
            activeHints[keyCode] = hint;

        // Ÿ�� ���󰡰� ����
        FollowTargetUI follow = ui.GetComponent<FollowTargetUI>();
        if (follow == null)
            follow = ui.AddComponent<FollowTargetUI>();

        follow.target = target;

        // ī�޶� �ٶ󺸵���
        if (ui.GetComponent<LookAtCamera>() == null)
            ui.AddComponent<LookAtCamera>();
    }



    public void RemoveHintByKey(string keyName)
    {
        if (!System.Enum.TryParse(keyName, true, out KeyCode keyCode))
        {
            Debug.LogError($"'{keyName}'�� KeyCode�� �νĵ��� �ʽ��ϴ�.");
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
