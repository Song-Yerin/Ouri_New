using System;
using System.Collections.Generic;
using UnityEngine;

public class PaperInventory : MonoBehaviour
{
    public static PaperInventory Instance { get; private set; }

    public event Action<PaperData, int> OnPieceAdded;
    public event Action<PaperData> OnPaperCompleted;
    public event Action<PaperData, int> OnPiecePlaced;
    public event Action<PaperData, int> OnPieceUnplaced;
    // public event Action<PaperData> OnPaperCompletedByPlacement; // ← 퍼즐 완성 트리거로 쓰지 않음(필요시 개별 사용)

    [Serializable] class Entry { public int id; public List<int> extraIdx = new(); }
    [Serializable] class EntryPlaced { public int id; public List<int> placed = new(); }
    [Serializable] class SlotPos { public int piece; public int slot; }
    [Serializable] class EntryPlacedSlots { public int id; public List<SlotPos> items = new(); }

    [Serializable]
    class SaveModel
    {
        public List<int> paperIds = new();
        public List<Entry> entries = new();
        public List<EntryPlaced> placedEntries = new();
        public List<EntryPlacedSlots> placedSlots = new();   // 슬롯별 배치 저장
    }

    HashSet<int> ownedPaperIds = new();
    Dictionary<int, HashSet<int>> ownedExtraIdxByPaper = new();
    Dictionary<int, HashSet<int>> placedExtraIdxByPaper = new();

    //“어느 슬롯에 꽂았는지” 저장: paperId -> (pieceIndex -> slotIndex)
    Dictionary<int, Dictionary<int, int>> placedSlotByPaper = new();

    const string SaveKey = "papers_v2";

#if UNITY_EDITOR
    public bool resetOnPlayInEditor = false;
#endif

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        if (resetOnPlayInEditor)
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            ownedPaperIds.Clear();
            ownedExtraIdxByPaper.Clear();
            placedExtraIdxByPaper.Clear();
            placedSlotByPaper.Clear(); // 초기화
        }
#endif
        Load();
    }

    public bool HasPaper(int id) => ownedPaperIds.Contains(id);
    public IEnumerable<int> GetAllPaperIds() => ownedPaperIds;

    public bool AddPaper(PaperData data)
    {
        if (!data) return false;
        bool added = ownedPaperIds.Add(data.id);
        if (!ownedExtraIdxByPaper.ContainsKey(data.id))
            ownedExtraIdxByPaper[data.id] = new HashSet<int>();
        if (!placedExtraIdxByPaper.ContainsKey(data.id))
            placedExtraIdxByPaper[data.id] = new HashSet<int>();
        if (!placedSlotByPaper.ContainsKey(data.id))         // 슬롯맵 준비
            placedSlotByPaper[data.id] = new Dictionary<int, int>();
        if (added) Save();
        return added;
    }

    public bool AddExtraSprite(PaperData data, int extraIndex)
    {
        if (!data || data.extraSprites == null) return false;
        if (extraIndex < 0 || extraIndex >= data.extraSprites.Length) return false;

        AddPaper(data);
        bool added = ownedExtraIdxByPaper[data.id].Add(extraIndex);
        if (added)
        {
            Save();
            OnPieceAdded?.Invoke(data, extraIndex);
            if (IsCompleted(data)) OnPaperCompleted?.Invoke(data);
        }
        return added;
    }

    public IReadOnlyCollection<int> GetOwnedExtraIndices(int paperId)
    {
        return ownedExtraIdxByPaper.TryGetValue(paperId, out var set)
            ? (IReadOnlyCollection<int>)set
            : Array.Empty<int>();
    }

    public IReadOnlyCollection<int> GetPlacedExtraIndices(int paperId)
    {
        return placedExtraIdxByPaper.TryGetValue(paperId, out var set)
            ? (IReadOnlyCollection<int>)set
            : Array.Empty<int>();
    }

    public bool SetPiecePlaced(PaperData data, int extraIndex, bool placed)
    {
        if (!data) return false;
        if (!ownedExtraIdxByPaper.TryGetValue(data.id, out var owned) || !owned.Contains(extraIndex))
            return false;

        if (!placedExtraIdxByPaper.TryGetValue(data.id, out var set))
            placedExtraIdxByPaper[data.id] = set = new HashSet<int>();

        bool changed = placed ? set.Add(extraIndex) : set.Remove(extraIndex);
        if (changed)
        {
            Save();
            if (placed) OnPiecePlaced?.Invoke(data, extraIndex);
            else OnPieceUnplaced?.Invoke(data, extraIndex);
        }
        return changed;
    }

    public bool IsCompleted(PaperData data)
    {
        if (!data || data.extraSprites == null || data.extraSprites.Length == 0) return false;
        return ownedExtraIdxByPaper.TryGetValue(data.id, out var set)
            && set.Count >= data.extraSprites.Length;
    }

    public bool IsCompletedByPlacement(PaperData data)
    {
        if (!data || data.extraSprites == null || data.extraSprites.Length == 0) return false;
        return placedExtraIdxByPaper.TryGetValue(data.id, out var set)
            && set.Count >= data.extraSprites.Length;
    }

    // “어느 슬롯에 꽂혔는지” 기록
    public void SetPiecePlacedInSlot(PaperData data, int pieceIndex, int slotIndex)
    {
        if (!data) return;
        if (!placedSlotByPaper.TryGetValue(data.id, out var map))
            placedSlotByPaper[data.id] = map = new Dictionary<int, int>();
        map[pieceIndex] = slotIndex;
        Save();
    }

    public bool TryGetPlacedSlotIndex(int paperId, int pieceIndex, out int slotIndex)
    {
        slotIndex = -1;
        return placedSlotByPaper.TryGetValue(paperId, out var map) && map.TryGetValue(pieceIndex, out slotIndex);
    }

    public void Save()
    {
        var model = new SaveModel();

        model.paperIds.AddRange(ownedPaperIds);

        foreach (var kv in ownedExtraIdxByPaper)
            model.entries.Add(new Entry { id = kv.Key, extraIdx = new List<int>(kv.Value) });

        foreach (var kv in placedExtraIdxByPaper)
            model.placedEntries.Add(new EntryPlaced { id = kv.Key, placed = new List<int>(kv.Value) });

        // 슬롯별 배치 저장
        foreach (var kv in placedSlotByPaper)
        {
            var e = new EntryPlacedSlots { id = kv.Key, items = new List<SlotPos>() };
            foreach (var kv2 in kv.Value)
                e.items.Add(new SlotPos { piece = kv2.Key, slot = kv2.Value });
            model.placedSlots.Add(e);
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(model));
        PlayerPrefs.Save();
    }

    public void Load()
    {
        ownedPaperIds.Clear();
        ownedExtraIdxByPaper.Clear();
        placedExtraIdxByPaper.Clear();
        placedSlotByPaper.Clear(); 

        var json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return;

        var model = JsonUtility.FromJson<SaveModel>(json);
        if (model == null) return;

        foreach (var id in model.paperIds) ownedPaperIds.Add(id);

        foreach (var e in model.entries)
            ownedExtraIdxByPaper[e.id] = new HashSet<int>(e.extraIdx);

        if (model.placedEntries != null)
            foreach (var e in model.placedEntries)
                placedExtraIdxByPaper[e.id] = new HashSet<int>(e.placed);

        // 슬롯별 배치 로드
        if (model.placedSlots != null)
        {
            foreach (var e in model.placedSlots)
            {
                var map = new Dictionary<int, int>();
                foreach (var it in e.items)
                    map[it.piece] = it.slot;
                placedSlotByPaper[e.id] = map;
            }
        }
    }
}
