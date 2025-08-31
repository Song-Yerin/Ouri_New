// PaperUIController.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PaperUIController : MonoBehaviour
{
    
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // �巡�� �� ������ �� blocksRaycasts�� false�� ���� ���� ����
        cg.blocksRaycasts = true;
        cg.interactable = true;

    }

    void Update()
    {
        // ESC / Delete / ��Ŭ�����ε� �ݱ�
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Delete) ||
            Input.GetMouseButtonDown(1))
        {
            Close();
        }
    }

    public void Close()
    {
        Destroy(gameObject);  // �ν��Ͻ��� ���� (�κ��丮�� ����)
    }
}
