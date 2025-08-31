using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VIDE_Data;

public class DialogueTrigger : MonoBehaviour
{
    public Test_UIManager dialogueManager;
    public VIDE_Assign myDialogue;

    //— 세이브 파일명과 ExtraVariables를 쓸 노드 ID
    const string saveFile = "dialogue_state";
    const int extraNodeID = 4;

    void Awake()
    {
        // Node가 바뀔 때마다 호출될 콜백 등록
        VD.OnNodeChange += HandleNodeChange;
        // 액션 노드 실행 시 호출될 콜백 등록
        VD.OnActionNode += OnActionNodeTriggered;
    }

    void OnDestroy()
    {
        VD.OnNodeChange -= HandleNodeChange;
        VD.OnActionNode -= OnActionNodeTriggered;
    }

    void Start()
    {
        StartCoroutine(InitDialogue());
    }

    IEnumerator InitDialogue()
    {
        yield return null;
        VD.LoadDialogues(myDialogue.GetAssigned());

        var dir = Path.Combine(Application.dataPath, "VIDE/saves");
        var vdPath = Path.Combine(dir, saveFile + ".json");
        var vaPath = Path.Combine(dir, "VA", saveFile + ".json");

        if (File.Exists(vdPath) && File.Exists(vaPath))
        {
            // 세이브가 있으면 로드
            VD.LoadState(saveFile, true);
            myDialogue.LoadState(saveFile);

            // angry 값 읽기
            var extraVars = VD.GetExtraVariables(myDialogue.GetAssigned(), extraNodeID);
            bool isAngry = false;
            if (extraVars != null && extraVars.TryGetValue("angry", out var raw))
            {
                bool.TryParse(raw.ToString(), out isAngry);
                Debug.Log($"▶ angry 플래그 값: {isAngry}");
            }
            else
            {
                Debug.Log("▶ angry 키 자체가 없거나, false 입니다.");
            }

            // angry가 false 면 0번 노드로 시작하도록 override
            if (!isAngry)
                myDialogue.overrideStartNode = 0;
            // else 기본 StartNode 사용
        }
        else
        {
            // 첫 실행: angry=false로 초기화 후 저장
            VD.SetExtraVariables(
                myDialogue.GetAssigned(),
                extraNodeID,
                new Dictionary<string, object> { { "angry", false } }
            );
            SaveDialogueState();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // dialogueManager.Interact(myDialogue);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 메모리에서 angry = false 덮어쓰기
            VD.SetExtraVariables(
                myDialogue.GetAssigned(),
                extraNodeID,
                new Dictionary<string, object> { { "angry", false } }
            );

            // 파일에 저장
            SaveDialogueState();

            // 즉시 다시 로드해서 메모리에 반영
            VD.LoadState(saveFile, true);
            myDialogue.LoadState(saveFile);

            // 상태 확인
            var check = VD.GetExtraVariables(myDialogue.GetAssigned(), extraNodeID);
            bool now = check != null
                       && check.TryGetValue("angry", out var v)
                       && bool.TryParse(v.ToString(), out var tmp)
                       && tmp;
            Debug.Log($"▶ Q 눌러서 angry를 false로 초기화했습니다. 현재 angry = {now}");
        }
    }

    // — Node가 바뀔 때마다 불립니다
    void HandleNodeChange(VD.NodeData data)
    {
        if (data.nodeID == 7)
        {
            var extra = VD.GetExtraVariables(myDialogue.GetAssigned(), extraNodeID);
            bool isAngry = extra != null
                           && extra.TryGetValue("angry", out var val)
                           && bool.TryParse(val.ToString(), out var tmp)
                           && tmp;

            Debug.Log($"[Debug] Node 7 도달! angry = {isAngry}");

            if (!isAngry)
            {
                VD.SetExtraVariables(
                    myDialogue.GetAssigned(),
                    extraNodeID,
                    new Dictionary<string, object> { { "angry", true } }
                );
                SaveDialogueState();
                Debug.Log("→ angry를 true로 변경하고 저장했습니다.");
            }
        }
    }

    // 액션 노드 실행 감지 핸들러
    void OnActionNodeTriggered(int nodeID)
    {
        if (nodeID == 8)
        {
            MyCustomAction();
        }
    }

    // 액션 노드 실행 시 호출할 함수
    void MyCustomAction()
    {
        Debug.Log("액션 노드 8번이 실행되었습니다. 추가 로직 실행!");
        // 여기에 원하는 추가 기능 구현
    }

    void SaveDialogueState()
    {
        var dir = Path.Combine(Application.persistentDataPath, "VIDE", "saves");
        var vaDir = Path.Combine(dir, "VA");
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(vaDir);

        VD.SaveState(saveFile, true);
        myDialogue.SaveState(saveFile);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        Debug.Log("▶ AssetDatabase.Refresh() 호출: JSON 에셋 갱신됨");
#endif
    }

    public void annoy()
    {
        myDialogue.overrideStartNode = 7;
    }
}
