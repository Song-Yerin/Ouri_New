using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VIDE_Data;

// 범용 대화 컨트롤러: 다양한 노드/액션 트리거, ExtraVariables 관리, 입력 처리
public class DialogueController : MonoBehaviour
{
    [Header("대화 참조")]
    public Test_UIManager dialogueManager;
    public VIDE_Assign dialogueAssign;

    [Header("저장 파일명 (확장자 없이)")]
    public string saveFile = "dialogue_state";
    [Header("ExtraVariables 저장에 사용할 노드 ID")]
    public int extraNodeID = 4;

    // 내부 이벤트 핸들러 저장
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
        // 키 입력 처리
        foreach (var (key, handler) in keyHandlers)
        {
            if (Input.GetKeyDown(key))
                handler?.Invoke();
        }
    }

    // 대화 및 상태 로드
    public IEnumerator InitDialogue(bool loadStateIfExists)
    {
        yield return null;
        VD.LoadDialogues(dialogueAssign.GetAssigned());

        if (loadStateIfExists && LoadStateIfExists())
            yield break;
    }

    // 저장된 파일이 있으면 불러옵니다
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

    // 상태 저장
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

    // 노드 변경 내부 핸들러
    private void OnNodeChangedInternal(VD.NodeData data)
    {
        if (nodeChangeHandlers.TryGetValue(data.nodeID, out var list))
            foreach (var h in list) h(data);
    }

    // 액션 노드 내부 핸들러
    private void OnActionNodeInternal(int nodeID)
    {
        if (actionNodeHandlers.TryGetValue(nodeID, out var list))
            foreach (var h in list) h();
    }

    // 노드 변경 트리거 등록
    public void RegisterNodeChange(int nodeID, Action<VD.NodeData> handler)
    {
        if (!nodeChangeHandlers.TryGetValue(nodeID, out var list))
            nodeChangeHandlers[nodeID] = list = new List<Action<VD.NodeData>>();
        list.Add(handler);
    }

    // 액션 노드 트리거 등록
    public void RegisterActionNode(int nodeID, Action handler)
    {
        if (!actionNodeHandlers.TryGetValue(nodeID, out var list))
            actionNodeHandlers[nodeID] = list = new List<Action>();
        list.Add(handler);
    }

    // 키 입력 트리거 등록
    public void RegisterKeyTrigger(KeyCode key, Action handler)
    {
        keyHandlers.Add((key, handler));
    }

    // ExtraVariables 값 설정 (문자열)
    public void SetExtraValue(string key, string value)
    {
        VD.SetExtraVariables(
            dialogueAssign.GetAssigned(),
            extraNodeID,
            new Dictionary<string, object> { { key, value } }
        );
    }

    // Generic으로 ExtraVariables 값 가져오기
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

    // 대화 시작 노드 강제 설정
    public void OverrideStartNode(int nodeID)
    {
        dialogueAssign.overrideStartNode = nodeID;
    }

    // 대화 시작 호출
    public void Interact()
    {
        dialogueManager.Interact(dialogueAssign);
    }
}
