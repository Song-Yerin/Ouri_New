
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyInputUI : MonoBehaviour
{
    public TextMeshProUGUI description;  // �߾� �ؽ�Ʈ
    public RectTransform keyIcon;        // ������ RectTransform
    public float spacing = 30f;          // �ؽ�Ʈ ���ʰ� ������ ���� �Ÿ�

    void LateUpdate()
    {
        if (description == null || keyIcon == null) return;

        RectTransform descRect = description.rectTransform;

        // �ؽ�Ʈ �߾� ���� �� ���� �� ��ǥ
        float textLeft = descRect.anchoredPosition.x - (description.preferredWidth * 0.5f);

        // �������� �� ���� spacing ��ŭ ����߷� ��ġ
        keyIcon.anchoredPosition = new Vector2(
            textLeft - spacing - (keyIcon.sizeDelta.x * 0.5f),
            descRect.anchoredPosition.y
        );
    }
}
