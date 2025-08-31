using UnityEngine;
using TMPro; // TextMeshProUGUI ���� ���

public class SegmentUIBinder : MonoBehaviour
{
    [SerializeField] private GimmickSegment segment;
    [SerializeField] private GameObject group;          // UI �׷� (�Ѱ� ���� ��)
    [SerializeField] private TextMeshProUGUI ringText;  // "0/0" ǥ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI lifeText;     // "x1" ����

    private void OnEnable()
    {
        if (!segment) segment = GetComponent<GimmickSegment>();
        if (segment)
        {
            segment.OnRingProgressChanged += HandleRings;
            segment.OnLivesChanged += HandleLives;
            // �ʱ� �ݿ�
            HandleRings(segment.ClearedCount, segment.TotalRings);
            HandleLives(segment.LivesLeft, segment.TotalLives);
        }
        if (group) group.SetActive(false); // ó���� ����
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

    // ���׸�Ʈ���� ȣ���� UI ���̱�/�����
    public void Show(bool on)
    {
        if (group) group.SetActive(on);
    }
}
