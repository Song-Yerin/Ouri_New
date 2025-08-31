using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VIDE_Data;

// ���� ��ȭ ��Ʈ�ѷ�: �پ��� ���/�׼� Ʈ����, ExtraVariables ����, �Է� ó��
public class DialogueController : MonoBehaviour
{
    [Header("��ȭ ����")]
    public Test_UIManager dialogueManager;
    public VIDE_Assign dialogueAssign;

    [Header("���� ���ϸ� (Ȯ���� ����)")]
    public string saveFile = "dialogue_state";
    [Header("ExtraVariables ���忡 ����� ��� ID")]
    public int extraNodeID = 4;

    // ���� �̺�Ʈ �ڵ鷯 ����
    private Dictionary<int, List<Action<VD.NodeData>>> nodeChangeHandlers = new Dictionary<int, List<Action<VD.NodeData>>>();
    private Dictionary<int, List<Action>> actionNodeHandlers = new Dictionary<int, List<Action>>();
    private List<(KeyCode key, Action handler)> keyHandlers = new List<(KeyCode, Action)>();

    void Awake()
    {
        VD.OnNodeChange += OnNodeChangedInternal;
        VD.OnActionNode += OnActionNodeInternal;
    }

    void OnDestroy()
    {
        VD.OnNodeChange -= OnNodeChangedInternal;
        VD.OnActionNode -= OnActionNodeInternal;
    }

    void Start()
    {
        StartCoroutine(InitDialogue(loadStateIfExists: true));
    }

    void Update()
    {
        // Ű �Է� ó��
        foreach (var (key, handler) in keyHandlers)
        {
            if (Input.GetKeyDown(key))
                handler?.Invoke();
        }
    }

    // ��ȭ �� ���� �ε�
    public IEnumerator InitDialogue(bool loadStateIfExists)
    {
        yield return null;
        VD.LoadDialogues(dialogueAssign.GetAssigned());

        if (loadStateIfExists && LoadStateIfExists())
            yield break;
    }

    // ����� ������ ������ �ҷ��ɴϴ�
    public bool LoadStateIfExists()
    {
        string dir = Path.Combine(Application.persistentDataPath, "VIDE", "saves");
        string vd = Path.Combine(dir, saveFile + ".json");
        string va = Path.Combine(dir, "VA", saveFile + ".json");
        if (File.Exists(vd) && File.Exists(va))
        {
            VD.LoadState(saveFile, true);
            dialogueAssign.LoadState(saveFile);
            return true;
        }
        return false;
    }

    // ���� ����
    public void SaveState()
    {
        string dir = Path.Combine(Application.persistentDataPath, "VIDE", "saves");
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "VA"));

        VD.SaveState(saveFile, true);
        dialogueAssign.SaveState(saveFile);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    // ��� ���� ���� �ڵ鷯
    private void OnNodeChangedInternal(VD.NodeData data)
    {
        if (nodeChangeHandlers.TryGetValue(data.nodeID, out var list))
            foreach (var h in list) h(data);
    }

    // �׼� ��� ���� �ڵ鷯
    private void OnActionNodeInternal(int nodeID)
    {
        if (actionNodeHandlers.TryGetValue(nodeID, out var list))
            foreach (var h in list) h();
    }

    // ��� ���� Ʈ���� ���
    public void RegisterNodeChange(int nodeID, Action<VD.NodeData> handler)
    {
        if (!nodeChangeHandlers.TryGetValue(nodeID, out var list))
            nodeChangeHandlers[nodeID] = list = new List<Action<VD.NodeData>>();
        list.Add(handler);
    }

    // �׼� ��� Ʈ���� ���
    public void RegisterActionNode(int nodeID, Action handler)
    {
        if (!actionNodeHandlers.TryGetValue(nodeID, out var list))
            actionNodeHandlers[nodeID] = list = new List<Action>();
        list.Add(handler);
    }

    // Ű �Է� Ʈ���� ���
    public void RegisterKeyTrigger(KeyCode key, Action handler)
    {
        keyHandlers.Add((key, handler));
    }

    // ExtraVariables �� ���� (���ڿ�)
    public void SetExtraValue(string key, string value)
    {
        VD.SetExtraVariables(
            dialogueAssign.GetAssigned(),
            extraNodeID,
            new Dictionary<string, object> { { key, value } }
        );
    }

    // Generic���� ExtraVariables �� ��������
    public T GetExtraValue<T>(string key, T defaultValue)
    {
        var dict = VD.GetExtraVariables(dialogueAssign.GetAssigned(), extraNodeID);
        if (dict != null && dict.TryGetValue(key, out var raw) && raw != null)
        {
            string s = raw.ToString();
            try
            {
                if (typeof(T) == typeof(int)) return (T)(object)int.Parse(s);
                if (typeof(T) == typeof(float)) return (T)(object)float.Parse(s);
                if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(s);
                if (typeof(T) == typeof(string)) return (T)(object)s;
                return (T)Convert.ChangeType(s, typeof(T));
            }
            catch { }
        }
        return defaultValue;
    }

    // ��ȭ ���� ��� ���� ����
    public void OverrideStartNode(int nodeID)
    {
        dialogueAssign.overrideStartNode = nodeID;
    }

    // ��ȭ ���� ȣ��
    public void Interact()
    {
        dialogueManager.Interact(dialogueAssign);
    }
}
