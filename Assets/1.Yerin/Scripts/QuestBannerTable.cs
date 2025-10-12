using UnityEngine;

[CreateAssetMenu(fileName = "QuestBannerTable", menuName = "Quest/QuestBannerTable")]
public class QuestBannerTable : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string sceneName;
        public string chapter; // e.g., "Ex"
        public string title;   // e.g., "인공위성 탐색하기"
    }
    public Entry[] entries;

    public bool TryGet(string sceneName, out Entry e)
    {
        foreach (var x in entries) if (x.sceneName == sceneName) { e = x; return true; }
        e = default; return false;
    }
}
