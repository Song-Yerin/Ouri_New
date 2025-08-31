
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyInputUI : MonoBehaviour
{
    public TextMeshProUGUI description;  // 중앙 텍스트
    public RectTransform keyIcon;        // 아이콘 RectTransform
    public float spacing = 30f;          // 텍스트 왼쪽과 아이콘 사이 거리

    void LateUpdate()
    {
        if (description == null || keyIcon == null) return;

        RectTransform descRect = description.rectTransform;

        // 텍스트 중앙 기준 → 왼쪽 끝 좌표
        float textLeft = descRect.anchoredPosition.x - (description.preferredWidth * 0.5f);

        // 아이콘을 그 왼쪽 spacing 만큼 떨어뜨려 배치
        keyIcon.anchoredPosition = new Vector2(
            textLeft - spacing - (keyIcon.sizeDelta.x * 0.5f),
            descRect.anchoredPosition.y
        );
    }
}
