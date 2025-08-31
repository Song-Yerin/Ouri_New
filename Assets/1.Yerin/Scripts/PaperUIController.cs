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
        // 드래그 중 끊겼을 때 blocksRaycasts가 false로 남는 문제 예방
        cg.blocksRaycasts = true;
        cg.interactable = true;

    }

    void Update()
    {
        // ESC / Delete / 우클릭으로도 닫기
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Delete) ||
            Input.GetMouseButtonDown(1))
        {
            Close();
        }
    }

    public void Close()
    {
        Destroy(gameObject);  // 인스턴스만 제거 (인벤토리엔 남음)
    }
}
