using System;
using System.Collections.Generic;
using UnityEngine;

public class PaperUISpawner : MonoBehaviour
{
    public DraggablePaperMin paperPrefab;
    public RectTransform workArea;

    // �κ��丮�� �ִ� PaperData�� ã�� ���� ��ü ���(Inspector���� ä����)
    public PaperData[] allPapers;

    // ��ǥ ��������Ʈ�� ��ĥ��, extraSprites���� ���� ��ĥ�� ����
    public bool includeExtraSprites = false;

    // --- ���� ���� ǥ�� ---
    public void Show(PaperData data)
    {
        var list = BuildSpriteList(data);
        ShowMany(list);
    }

    // --- B��: ������ ��� ���̸� �� ���� ---
    public void ShowAllOwned(bool clearExisting = true)
    {
        if (PaperInventory.Instance == null) return;
        if (clearExisting) CloseAllOpenPapers();

        var sprites = new List<Sprite>();
        foreach (var id in PaperInventory.Instance.GetAllPaperIds())
        {
            var data = System.Array.Find(allPapers, p => p.id == id);
            if (data == null) continue;


            // �� Inventory API�� ���� ������ �߰�
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

    // --- �����ִ� ���� UI ���� �ݱ� ---
    public void CloseAllOpenPapers()
    {
        foreach (var p in FindObjectsOfType<PaperUIController>(true))
            Destroy(p.gameObject);
    }

    // --- ���� ---
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

    // ���� ���� ���� ���� ���ļ� ����(������ �� �迭)
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
