using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VIDE_Data;

public class DialogueTriggerZone : MonoBehaviour
{
    public Test_UIManager dialogueManager;
    public VIDE_Assign myDialogue;

    const int extraNodeID = 4;
    private string saveFile = "";

    [Header("Next Section 이동 설정")]
    public float moveDistance = 5f; // 인스펙터에서 지정할 이동 거리

    // 추가: 토글/플레이어 인식 옵션
    [Header("대화 시작 방식 토글")]
    public bool autoStartOnEnter = true;   // true: 존 진입 시 자동 시작, false: 존 안에서 E키로 시작
    public bool endOnExit = false;         // 존에서 나가면 대화 종료할지

    [Header("플레이어 인식")]
    public string playerTag = "Player";    // 플레이어 태그

    // 추가: 내부 상태
    private bool playerInside = false;     // 플레이어가 존 내부인지
    private bool hasTriggered = false;     // 자동 시작 중복 방지

    [Header("UI 이름 오버라이드")]
    public bool applyNameOverridesOnEnter = false; // 체크하면 진입 시 오버라이드 적용
    public string playerNameOverride;              // 비어있지 않으면 적용
    public string npcNameOverride;                 // 비어있지 않으면 적용

    public bool restoreNamesOnExit = false;        // 존에서 나갈 때 원래 값으로 되돌릴지

    // 내부 백업
    private string cachedPlayerName;
    private string cachedNPCName;
    private bool cachedSaved = false;

    public int nightMapProgress = 0;
    // 0 = 처음, 1 = 첫 대화 끝, 2 = 다음 이벤트 … 이런 식으로 사용

    // Next Section 타겟 포인트
    [Header("Next Section 타겟 포인트")]
    public List<Transform> targetPoints = new List<Transform>();

    [Tooltip("이동할 타겟 인덱스 (0 기반)")]
    public int targetIndex = 0;

    [Tooltip("타겟의 회전까지 맞출지 여부")]
    public bool matchRotation = false;

    [Tooltip("타겟 위치에 더할 오프셋(선택)")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("NPC Sprite 교체")]
    public Sprite npcSpriteForProgress2;

    [Header("체크포인트 포인트들")]
    public List<Transform> checkpointPoints = new List<Transform>();
    public Transform player; // 이동시킬 플레이어(인스펙터에서 연결)


    void Awake()
    {
        VD.OnNodeChange += HandleNodeChange;
        VD.OnActionNode += OnActionNodeTriggered;
    }

    void OnDestroy()
    {
        VD.OnNodeChange -= HandleNodeChange;
        VD.OnActionNode -= OnActionNodeTriggered;
    }

    void Start()
    {
        // 추가: 트리거 보장
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

        if (myDialogue != null && myDialogue.GetAssigned() != null)
            saveFile = myDialogue.GetAssigned();
        else
            saveFile = "default_dialogue";
        nightMapProgress = PlayerPrefs.GetInt("nightMapProgress", nightMapProgress);
        Debug.Log(nightMapProgress);

        // ★ 씬 시작 시 체크포인트 위치로 플레이어 이동
        MovePlayerToCheckpoint(nightMapProgress);

        StartCoroutine(InitDialogue());
    }

    void MovePlayerToCheckpoint(int progress)
    {
        if (player == null) return;

        if (progress >= 0 && progress < checkpointPoints.Count)
        {
            Transform cp = checkpointPoints[progress];
            if (cp != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;  // 비활성화 후 위치 옮기기
                    player.position = cp.position;
                    player.rotation = cp.rotation;
                    cc.enabled = true;   // 다시 켜기
                }
                else
                {
                    player.position = cp.position;
                    player.rotation = cp.rotation;
                }

                Debug.Log($"체크포인트 {progress} 위치로 플레이어 이동 완료: {cp.name}");
            }
        }
    }

    IEnumerator InitDialogue()
    {
        yield return null;

        if (myDialogue == null || myDialogue.GetAssigned() == null)
        {
            Debug.LogError("VIDE_Assign이 연결되지 않았습니다!");
            yield break;
        }

        // 대화 파일 로드
        VD.LoadDialogues(myDialogue.GetAssigned());

        // 세이브 파일 경로
        var dir = Path.Combine(Application.persistentDataPath, "VIDE", "saves");
        var vdPath = Path.Combine(dir, saveFile + ".json");
        var vaPath = Path.Combine(dir, "VA", saveFile + ".json");

        if (File.Exists(vdPath) && File.Exists(vaPath))
        {
            VD.LoadState(saveFile, true);
            myDialogue.LoadState(saveFile);
        }
        else
        {
            SaveDialogueState();
        }

        // 추가: 시작 노드가 0이면 자동 시작 켜기
        SyncAutoStartFromStartNode();

        // ★ 진행도 반영
        ApplyProgress();
    }

    void Update()
    {
        // 추가: 자동 시작 모드일 때, 존 내부에서 1회 자동 시작
        if (!VD.isActive && autoStartOnEnter && playerInside && !hasTriggered)
        {
            hasTriggered = true;
            dialogueManager.Interact(myDialogue);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 대화 미시작 상태에서만 시작 조건을 제한
            if (!VD.isActive)
            {
                // 자동 시작이 꺼져 있을 때만, 존 내부에서 E로 시작
                if (!autoStartOnEnter && playerInside)
                {
                    dialogueManager.Interact(myDialogue);
                }
                // 자동 시작 모드일 때는 E로 시작하지 않음(진입 시 자동 시작)
            }
            else
            {
                // 대화 진행 중이면 E로 다음 진행
                dialogueManager.Interact(myDialogue);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (nightMapProgress == 0)
            {
                SetProgress(1);
            }
            else if (nightMapProgress == 1)
            {
                SetProgress(0);
            }
        }
    }

    // 추가: 트리거 진입/이탈
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        // 이름 오버라이드 적용
        if (applyNameOverridesOnEnter && dialogueManager != null)
        {
            if (!cachedSaved)
            {
                cachedPlayerName = dialogueManager.defaultPlayerName;
                cachedNPCName = dialogueManager.defaultNPCName;
                cachedSaved = true;
            }

            if (!string.IsNullOrEmpty(playerNameOverride))
                dialogueManager.defaultPlayerName = playerNameOverride;

            if (!string.IsNullOrEmpty(npcNameOverride))
                dialogueManager.defaultNPCName = npcNameOverride;
        }

        // 자동 시작이 켜져 있으면 기존 로직대로 한 번만 시작
        if (!VD.isActive && autoStartOnEnter && !hasTriggered)
        {
            hasTriggered = true;
            dialogueManager.Interact(myDialogue);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        hasTriggered = false;

        if (restoreNamesOnExit && dialogueManager != null && cachedSaved)
        {
            dialogueManager.defaultPlayerName = cachedPlayerName;
            dialogueManager.defaultNPCName = cachedNPCName;
            // cachedSaved 유지해서 다음에도 복원 가능하게 두거나, 한번만 복원하려면 false로
            // cachedSaved = false;
        }

        if (endOnExit && VD.isActive)
        {
            VD.EndDialogue();
        }
    }


    void HandleNodeChange(VD.NodeData data)
    {
        if (data.nodeID == 2)
        {
            SetProgress(1);
        }

        if(data.nodeID == 16)
        {
            SetProgress(2, false);
        }
    }

    void OnActionNodeTriggered(int nodeID)
    {
        if (nodeID == 8)
        {
            MyCustomAction();
        }
    }

    void MyCustomAction()
    {
        Debug.Log("액션 노드 8번이 실행되었습니다!");
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
#endif
        Debug.Log("▶ 대화 상태 저장 완료");
    }

    public void nextsection()
    {
        if (targetPoints == null || targetPoints.Count == 0)
        {
            Debug.LogWarning("NextSection: targetPoints가 비어 있습니다. 인스펙터에서 타겟을 하나 이상 넣어주세요.");
            return;
        }

        // nightMapProgress 값을 인덱스로 사용
        int index = nightMapProgress;

        if (index < 0 || index >= targetPoints.Count)
        {
            Debug.LogWarning($"NextSection: nightMapProgress {index}가 범위를 벗어났습니다. 0 ~ {targetPoints.Count - 1} 사이로 설정하세요.");
            return;
        }

        Transform t = targetPoints[index];
        if (t == null)
        {
            Debug.LogWarning("NextSection: 지정된 타겟이 null 입니다. 리스트 항목을 확인하세요.");
            return;
        }

        Vector3 dest = t.position + positionOffset;

        if (matchRotation)
            transform.SetPositionAndRotation(dest, t.rotation);
        else
            transform.position = dest;

        Debug.Log($"▶ NextSection: nightMapProgress={index}, 타겟 '{t.name}'로 순간이동 완료. dest={dest}");
    }

    // 추가: 시작 노드 기반 자동 시작 토글 동기화
    void SyncAutoStartFromStartNode()
    {
        if (myDialogue != null)
        {
            if (myDialogue.overrideStartNode == 0)
                autoStartOnEnter = true;
            dialogueManager.defaultNPCName = "실험용쥐";
        }
    }

    // ★ 진행도 반영
    void ApplyProgress()
    {
        switch (nightMapProgress)
        {
            case 0:
                Debug.Log("진행도 0: 초기 상태");
                nextsection();
                myDialogue.overrideStartNode = 0;
                autoStartOnEnter = true; // 추가: intro 시작 시 자동 시작 켬
                hasTriggered = false;
                break;

            case 1:
                nextsection();
                myDialogue.overrideStartNode = 3;
                autoStartOnEnter = false; // 추가: intro 이후 자동 시작 끔
                hasTriggered = false;
                Debug.Log("진행도 1: Intro 이후 상태 반영 완료");
                break;

            case 2:
                nextsection();
                myDialogue.overrideStartNode = 17;
                dialogueManager.defaultNPCName = "부엉이";
                autoStartOnEnter = false; // 추가: 자동 시작 끔
                // ★ Default NPC Sprite 교체
                if (npcSpriteForProgress2 != null)
                {
                    dialogueManager.defaultNPCSprite = npcSpriteForProgress2;
                    Debug.Log("진행도 2: NPC 스프라이트 교체 완료");
                }
                hasTriggered = false;
                Debug.Log("진행도 2: 다음 이벤트 상태 반영 완료");
                break;

            default:
                Debug.LogWarning($"ApplyProgress: 처리되지 않은 nightMapProgress={nightMapProgress}");
                break;
        }
    }

    // ★ 진행도 갱신
    void SetProgress(int newValue, bool applyNow = true)
    {
        nightMapProgress = newValue;
        PlayerPrefs.SetInt("nightMapProgress", nightMapProgress);
        PlayerPrefs.Save();
        Debug.Log($"진행도 갱신: nightMapProgress={nightMapProgress}");

        if (applyNow)
            ApplyProgress();
    }

}
