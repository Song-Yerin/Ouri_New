using UnityEngine;
using TMPro; // TextMeshProUGUI 쓰는 경우

public class SegmentUIBinder : MonoBehaviour
{
    [SerializeField] private GimmickSegment segment;
    [SerializeField] private GameObject group;          // UI 그룹 (켜고 끄기 용)
    [SerializeField] private TextMeshProUGUI ringText;  // "0/0" 표시할 텍스트
    [SerializeField] private TextMeshProUGUI lifeText;     // "x1" 형태

    private void OnEnable()
    {
        if (!segment) segment = GetComponent<GimmickSegment>();
        if (segment)
        {
            segment.OnRingProgressChanged += HandleRings;
            segment.OnLivesChanged += HandleLives;
            // 초기 반영
            HandleRings(segment.ClearedCount, segment.TotalRings);
            HandleLives(segment.LivesLeft, segment.TotalLives);
        }
        if (group) group.SetActive(false); // 처음엔 숨김
    }

    private void OnDisable()
    {
        if (!segment) return;
        segment.OnRingProgressChanged -= HandleRings;
        segment.OnLivesChanged -= HandleLives;
    }


    private void HandleRings(int cleared, int total)
    {
        if (ringText) ringText.text = $"{cleared}/{total}";
    }

    private void HandleLives(int left, int total)
    {
        if (lifeText) lifeText.text = $"x{left}";
    }

    // 세그먼트에서 호출해 UI 보이기/숨기기
    public void Show(bool on)
    {
        if (group) group.SetActive(on);
    }
}
