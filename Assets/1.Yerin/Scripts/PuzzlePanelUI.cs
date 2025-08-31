using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PuzzlePanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform workArea;   // WorkArea RectTransform
    [SerializeField] Transform pieceContainer; // 조각 부모
    [SerializeField] Image completedPreview;   // 완성본
    [SerializeField] DropSlot[] slots;         // 4칸 (acceptPieceIndex = 0..3)

    [Header("Data/Prefab")]
    [SerializeField] PaperData currentData;    // 현재 퍼즐 대상
    [SerializeField] GameObject piecePrefab;   // Image + CanvasGroup + PaperPieceMeta + (드래그 스크립트)
                                               
    [SerializeField] bool fitToArea = false;                 // true면 fitArea에 맞춤, false면 maxSize 기준
    [SerializeField] RectTransform fitArea;
    [SerializeField] Vector2 maxSize = new Vector2(320, 320);
    [SerializeField] Vector2 areaPadding = new Vector2(8, 8); // fitArea 사용 시 안쪽 여백

    [Header("Options")]
    [SerializeField] bool autoSliceIfMissing = true;                  // extraSprites 없으면 런타임 2x2
    [SerializeField] bool useFixedPieceSize = true;                   // 조각 고정 크기
    [SerializeField] Vector2 pieceSize = new Vector2(140, 140);      // 조각 크기
    [SerializeField] bool preserveAspect = true;

    [Header("Spawn")]
    [SerializeField] RectTransform spawnZone;   // ← TopSpawnZone 
    [SerializeField] int spawnCols = 2;         // 스폰 그리드 가로 칸 수
    [SerializeField] float spawnPadding = 10f;  // 스폰 존 안쪽 여백


    bool subscribed;

    void OnEnable()
    {
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<DropSlot>(true);

        if (!subscribed && PaperInventory.Instance != null)
        {
            PaperInventory.Instance.OnPieceAdded += OnPieceAdded;
            subscribed = true;
        }

        if (currentData != null) RefreshAll();
    }

    void OnDisable()
    {
        if (subscribed && PaperInventory.Instance != null)
        {
            PaperInventory.Instance.OnPieceAdded -= OnPieceAdded;
            subscribed = false;
        }
    }

    void OnPieceAdded(PaperData d, int idx)
    {
        if (d == currentData) RefreshAll();
    }

    public void SetPaper(PaperData d)
    {
        currentData = d;
        RefreshAll();
    }

    //  배치 기록 보존
    void ClearAllPiecesAndSlots()
    {
        foreach (var m in workArea.GetComponentsInChildren<PaperPieceMeta>(true))
            Destroy(m.gameObject);

        // 드래그 스크립트로 만든 조각도 싹 제거(있을 때만)
        foreach (var d in workArea.GetComponentsInChildren<UIDragClamp>(true))
            Destroy(d.gameObject);
        foreach (var d in workArea.GetComponentsInChildren<DraggablePaperMin>(true))
            Destroy(d.gameObject);

        // 슬롯은 배치 해제하지 말고 시각만 초기화
        foreach (var s in slots) if (s) s.ClearVisualOnly();
    }

    public void RefreshAll()
    {
        if (!currentData || !piecePrefab || !workArea || !pieceContainer || !completedPreview) return;

        ClearAllPiecesAndSlots(); // 겹침 방지

        // 슬롯 수집/셋업
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<DropSlot>(true);

        foreach (var s in slots)
        {
            if (!s) continue;
            s.targetData = currentData;
            s.SetPiecesRoot((RectTransform)pieceContainer); // 스냅 후 부모 고정
            s.owner = this;
            s.gameObject.SetActive(true);
        }

        var pieces = GetPieces(currentData);
        if (pieces == null || pieces.Length < 4) return;

        var owned = PaperInventory.Instance?.GetOwnedExtraIndices(currentData.id) ?? System.Array.Empty<int>();
        var placed = PaperInventory.Instance?.GetPlacedExtraIndices(currentData.id) ?? System.Array.Empty<int>();

        int i = 0;
        foreach (var idx in owned)
        {
            if (idx < 0 || idx >= pieces.Length) continue;

            // ---- 프리팹 생성 & 이미지 세팅 ----
            var go = Instantiate(piecePrefab, pieceContainer);
            var img = go.GetComponent<Image>();
            img.sprite = pieces[idx];
            img.SetNativeSize();

            // 알파 히트(읽기 가능한 텍스처만)
            try
            {
                var sp = img.sprite;
                img.alphaHitTestMinimumThreshold = (sp && sp.texture && sp.texture.isReadable) ? 0.1f : 0f;
            }
            catch { img.alphaHitTestMinimumThreshold = 0f; }

            // 메타/드래그/사이즈
            var meta = go.GetComponent<PaperPieceMeta>();
            if (meta) { meta.data = currentData; meta.pieceIndex = idx; }

            var dragClamp = go.GetComponent<UIDragClamp>();
            if (dragClamp) dragClamp.SetArea(workArea); // or pieceContainer

            var rt = img.rectTransform;
            if (useFixedPieceSize && img.sprite != null)
            {
                var src = img.sprite.rect.size;
                if (preserveAspect)
                {
                    float k = Mathf.Min(pieceSize.x / src.x, pieceSize.y / src.y);
                    rt.sizeDelta = src * k;
                }
                else rt.sizeDelta = pieceSize;
            }
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;

            // ---- 복원: 저장된 슬롯 인덱스로 우선 복원 ----
            bool restored = false;
            int savedSlot;
            DropSlot slotToUse = null;

            if (PaperInventory.Instance != null &&
                PaperInventory.Instance.TryGetPlacedSlotIndex(currentData.id, idx, out savedSlot) &&
                savedSlot >= 0)
            {
                slotToUse = slots.FirstOrDefault(s => s && s.acceptPieceIndex == savedSlot);
            }

            if (slotToUse != null)
            {
                slotToUse.SnapFromScript(rt, meta);   // 저장된 "그 슬롯"으로 복원
                if (dragClamp) dragClamp.enabled = false;
                restored = true;
            }
            else if (placed.Contains(idx))
            {
                // 구버전 호환: 배치 플래그는 있으나 슬롯 인덱스가 없던 세이브일 때
                var fallback = slots.FirstOrDefault(s => s && s.acceptPieceIndex == idx);
                if (fallback != null)
                {
                    fallback.SnapFromScript(rt, meta);
                    if (dragClamp) dragClamp.enabled = false;
                    restored = true;
                }
            }

            // ---- 스폰: 복원 실패 시 스폰존에 배치 ----
            if (!restored)
            {
                var zone = spawnZone ? spawnZone : (RectTransform)pieceContainer;
                var zr = zone.rect; // pivot=0.5 가정

                int total = owned.Count;
                int cols = Mathf.Max(1, spawnCols);
                int rows = Mathf.Max(1, Mathf.CeilToInt(total / (float)cols));

                int col = i % cols;
                int row = i / cols;

                float cellW = (zr.width - spawnPadding * (cols + 1)) / cols;
                float cellH = (zr.height - spawnPadding * (rows + 1)) / rows;
                float cell = Mathf.Min(cellW, cellH);

                float x = -zr.width * 0.5f + spawnPadding + (col + 0.5f) * (cell + spawnPadding);
                float y = zr.height * 0.5f - spawnPadding - (row + 0.5f) * (cell + spawnPadding);

                Vector3 world = zone.TransformPoint(new Vector3(x, y, 0));
                Vector3 local = ((RectTransform)pieceContainer).InverseTransformPoint(world);
                rt.anchoredPosition = new Vector2(local.x, local.y);
                i++;
            }
        }

        completedPreview.gameObject.SetActive(false);
    }


    Sprite[] GetPieces(PaperData d)
    {
        if (!d) return null;
        if (d.extraSprites != null && d.extraSprites.Length >= 4)
            return d.extraSprites;

        if (!autoSliceIfMissing || d.sprite == null) return null;

        var src = d.sprite;
        var tex = src.texture;
        var r = src.rect; 
        float w = Mathf.Floor(r.width / 2f);
        float h = Mathf.Floor(r.height / 2f);
        float ppu = src.pixelsPerUnit;

        var tl = new Rect(r.x, r.y + h, w, h);
        var tr = new Rect(r.x + w, r.y + h, w, h);
        var bl = new Rect(r.x, r.y, w, h);
        var br = new Rect(r.x + w, r.y, w, h);

        return new[]{
            Sprite.Create(tex, tl, new Vector2(0.5f,0.5f), ppu), // 0: 좌상
            Sprite.Create(tex, tr, new Vector2(0.5f,0.5f), ppu), // 1: 우상
            Sprite.Create(tex, bl, new Vector2(0.5f,0.5f), ppu), // 2: 좌하
            Sprite.Create(tex, br, new Vector2(0.5f,0.5f), ppu), // 3: 우하
        };
    }

    public void CheckCompletion(PaperData d)
    {
        if (d != currentData) return;
        if (slots == null || slots.Length == 0) return;

        // 현재 퍼즐에 해당하는 "활성 슬롯"만 검사 (혹시 비활성 슬롯이 배열에 섞여 있어도 안전)
        foreach (var s in slots)
        {
            if (s == null) return;
            if (!s.gameObject.activeInHierarchy) return;
            if (s.targetData != currentData) return; // 다른 퍼즐 슬롯 섞임 방지

            if (!s.HasCorrectPiece())
            {
                // 필요하면 디버깅:
                // Debug.Log($"[PUZZLE] not correct: slot {s.acceptPieceIndex}");
                return;
            }
        }

        ShowCompleted(); // 네 칸 전부 정답일 때만
    }

    void ShowCompleted()
    {
        // 조각/슬롯 숨김
        foreach (Transform c in pieceContainer) c.gameObject.SetActive(false);
        foreach (var s in slots) if (s) s.gameObject.SetActive(false);

        // 스프라이트 결정
        var sp = currentData.sprite;
        if (!sp && currentData.extraSprites != null && currentData.extraSprites.Length > 0)
            sp = currentData.extraSprites[0];
        if (!sp) return;

        // 이미지 표시
        completedPreview.gameObject.SetActive(true);
        completedPreview.sprite = sp;
        completedPreview.preserveAspect = preserveAspect;  // Image에도 preserveAspect가 있어요
        completedPreview.SetNativeSize();

        // ---- 크기 계산 ----
        var rt = completedPreview.rectTransform;
        Vector2 src = sp.rect.size;

        Vector2 targetBox;
        if (fitToArea && fitArea != null)
        {
            // fitArea의 내부 크기(패딩 반영)
            var r = fitArea.rect;
            targetBox = new Vector2(Mathf.Max(0, r.width - areaPadding.x * 2f),
                                    Mathf.Max(0, r.height - areaPadding.y * 2f));

            // 위치도 fitArea 중심에 맞추고 싶다면:
            Vector3 worldCenter = fitArea.TransformPoint(r.center);
            Vector3 localCenter = ((RectTransform)completedPreview.transform.parent).InverseTransformPoint(worldCenter);
            rt.anchoredPosition = (Vector2)localCenter;
        }
        else
        {
            targetBox = maxSize;
            rt.anchoredPosition = Vector2.zero; // 부모 중앙
        }

        if (preserveAspect)
        {
            float k = Mathf.Min(
                targetBox.x / Mathf.Max(1, src.x),
                targetBox.y / Mathf.Max(1, src.y)
            );
            rt.sizeDelta = src * k;
        }
        else
        {
            rt.sizeDelta = targetBox; // 비율 무시하고 딱 채우기
        }

        // 필요하면 살짝 연출
        // rt.localScale = Vector3.one * 0.95f;  // 시작 스케일
        // StartCoroutine(Bounce(rt));           // 바운스 연출 등
    }

}

