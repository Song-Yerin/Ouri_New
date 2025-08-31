// PaperData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Paper/PaperData")]
public class PaperData : ScriptableObject
{
    public int id;
    public Sprite sprite;           // 아이콘/대표 이미지 (기존 그대로)
    public Sprite[] extraSprites;   // 추가로 함께 띄울 이미지들
}
