using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PaperPickup3D : MonoBehaviour
{
    [Header("What to give")]
    public PaperData data;                 // 반드시 같은 PaperData(완성본+extra 4장)
    [Tooltip("-1 = 대표, 0..3 = 조각 인덱스")]
    public int extraIndex = -1;

    [Header("UI Targets")]
    public PuzzlePanelUI puzzlePanel;      // WorkArea에 붙은 퍼즐 패널
    public PaperUISpawner spawner;         // (선택) 대표를 중앙 팝업으로 보여줄 때만

    [Header("Options")]
    public bool hide3DOnPickup = true;
    public bool openPanelOnPickup = true;

    bool inRange, picked;

    void Awake()
    {
        var col = GetComponent<Collider>(); col.isTrigger = true;
    }

    void Start()
    {
#if UNITY_2023_1_OR_NEWER
        if (!puzzlePanel) puzzlePanel = UnityEngine.Object.FindAnyObjectByType<PuzzlePanelUI>(UnityEngine.FindObjectsInactive.Include);
#else
        if (!puzzlePanel) puzzlePanel = UnityEngine.Object.FindObjectOfType<PuzzlePanelUI>(true);
#endif
        if (!spawner) spawner = FindObjectOfType<PaperUISpawner>(true);
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) { inRange = true; Debug.Log("[Pickup] Player in range"); } }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) { inRange = false; Debug.Log("[Pickup] Player out"); } }

    void Update()
    {
        if (!inRange || picked || !Input.GetKeyDown(KeyCode.R)) return;
        picked = true;

        if (PaperInventory.Instance == null) { Debug.LogError("[Pickup] PaperInventory.Instance == null"); return; }
        if (data == null) { Debug.LogError("[Pickup] data == null"); return; }

        if (extraIndex < 0)
        {
            PaperInventory.Instance.AddPaper(data);
            Debug.Log($"[Pickup] AddPaper (id={data.id})");
            if (spawner) { spawner.CloseAllOpenPapers(); spawner.Show(data); }
        }
        else
        {
            PaperInventory.Instance.AddExtraSprite(data, extraIndex);
            Debug.Log($"[Pickup] AddExtraSprite (id={data.id}, piece={extraIndex})");

            if (puzzlePanel)
            {
                if (openPanelOnPickup && !puzzlePanel.gameObject.activeSelf) puzzlePanel.gameObject.SetActive(true);
                puzzlePanel.SetPaper(data);   // ← 여기서 패널이 조각 생성
                Debug.Log("[Pickup] Called puzzlePanel.SetPaper");
            }
            else Debug.LogWarning("[Pickup] puzzlePanel is null");
        }

        if (hide3DOnPickup) gameObject.SetActive(false);
    }
}


