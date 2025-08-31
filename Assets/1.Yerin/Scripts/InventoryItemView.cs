// InventoryItemView.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemView : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    private PaperData data;
    private PaperUISpawner spawner;

    public void Init(PaperData d, PaperUISpawner s)
    {
        data = d;
        spawner = s;
        if (icon) icon.sprite = d.sprite;   // 아이콘 그림 적용
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (spawner != null && data != null)
            spawner.Show(data);             // PaperData 그대로 전달
    }
}

