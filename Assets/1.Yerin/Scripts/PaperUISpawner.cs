using System;
using System.Collections.Generic;
using UnityEngine;

public class PaperUISpawner : MonoBehaviour
{
    public DraggablePaperMin paperPrefab;
    public RectTransform workArea;

    // 인벤토리에 있는 PaperData를 찾기 위한 전체 목록(Inspector에서 채워줘)
    public PaperData[] allPapers;

    // 대표 스프라이트만 펼칠지, extraSprites까지 전부 펼칠지 선택
    public bool includeExtraSprites = false;

    // --- 기존 단일 표시 ---
    public void Show(PaperData data)
    {
        var list = BuildSpriteList(data);
        ShowMany(list);
    }

    // --- B안: 보유한 모든 종이를 한 번에 ---
    public void ShowAllOwned(bool clearExisting = true)
    {
        if (PaperInventory.Instance == null) return;
        if (clearExisting) CloseAllOpenPapers();

        var sprites = new List<Sprite>();
        foreach (var id in PaperInventory.Instance.GetAllPaperIds())
        {
            var data = System.Array.Find(allPapers, p => p.id == id);
            if (data == null) continue;


            // 새 Inventory API로 보유 조각만 추가
            var ownedIdx = PaperInventory.Instance.GetOwnedExtraIndices(id);
            if (data.extraSprites != null)
            {
                foreach (var idx in ownedIdx)
                {
                    if (idx >= 0 && idx < data.extraSprites.Length && data.extraSprites[idx] != null)
                        sprites.Add(data.extraSprites[idx]);
                }
            }
        }
        ShowMany(sprites);
    }

    // --- 열려있는 종이 UI 전부 닫기 ---
    public void CloseAllOpenPapers()
    {
        foreach (var p in FindObjectsOfType<PaperUIController>(true))
            Destroy(p.gameObject);
    }

    // --- 헬퍼 ---
    List<Sprite> BuildSpriteList(PaperData data)
    {
        var list = new List<Sprite>();
        if (data.sprite) list.Add(data.sprite);
        if (includeExtraSprites && data.extraSprites != null)
        {
            foreach (var s in data.extraSprites)
                if (s) list.Add(s);
        }
        return list;
    }

    // 여러 장을 보기 좋게 펼쳐서 스폰(간단한 팬 배열)
    public void ShowMany(List<Sprite> sprites)
    {
        if (paperPrefab == null || workArea == null || sprites == null || sprites.Count == 0) return;

        var center = Vector2.zero;
        float step = 64f;
        float rot  = 6f;

        for (int i = 0; i < sprites.Count; i++)
        {
            var ui = Instantiate(paperPrefab, workArea);
            ui.workArea = workArea;
            ui.Init(sprites[i]);

            var rt = ui.GetComponent<RectTransform>();
            float offset = (i - (sprites.Count - 1) * 0.5f) * step;
            rt.anchoredPosition  = center + new Vector2(offset, -offset * 0.3f);
            rt.localScale        = Vector3.one;
            rt.localEulerAngles  = new Vector3(0, 0, -rot * (i - (sprites.Count - 1) * 0.5f));
        }
    }
}
