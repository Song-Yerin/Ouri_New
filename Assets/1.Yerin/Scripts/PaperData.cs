// PaperData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Paper/PaperData")]
public class PaperData : ScriptableObject
{
    public int id;
    public Sprite sprite;           // ������/��ǥ �̹��� (���� �״��)
    public Sprite[] extraSprites;   // �߰��� �Բ� ��� �̹�����
}
