using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class LookUpCamUI : MonoBehaviour
{
    [Header("VCam (Body=Do Nothing, Aim=Do Nothing, 기본 비활성화)")]
    public CinemachineVirtualCamera lookUpCam;

    [Header("UI")]
    public CanvasGroup zoneUI;
    public Button btnLookUp;   // 위로 보기
    public Button btnRestore;  // 원래대로

    private Transform _currentViewPoint; // 존이 넘겨주는 카메라 자리(선택)

    void Awake()
    {
        if (btnLookUp) btnLookUp.onClick.AddListener(() => SetLookUp(true));
        if (btnRestore) btnRestore.onClick.AddListener(() => SetLookUp(false));
        SetLookUp(false); // 기본 OFF
        HideUI();
    }

    void Update()
    {
        // 화살표 키 입력으로 제어
        if (zoneUI && zoneUI.interactable)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                SetLookUp(true);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                SetLookUp(false);
        }
    }

    // 존 트리거에서 호출
    public void EnterZone(Transform viewPoint)
    {
        _currentViewPoint = viewPoint;
        ShowUI();
    }

    public void ExitZone(Transform viewPoint)
    {
        if (_currentViewPoint == viewPoint)
        {
            _currentViewPoint = null;
            HideUI();
            SetLookUp(false); // 존 나가면 자동 복귀
        }
    }

    private void SetLookUp(bool on)
    {
        if (!lookUpCam) return;

        if (on)
        {
            if (_currentViewPoint)
                lookUpCam.transform.SetPositionAndRotation(_currentViewPoint.position, _currentViewPoint.rotation);

            lookUpCam.m_Lens.FieldOfView = 60f;
            lookUpCam.gameObject.SetActive(true);

            if (btnLookUp) btnLookUp.gameObject.SetActive(false);
            if (btnRestore) btnRestore.gameObject.SetActive(true);
        }
        else
        {
            lookUpCam.gameObject.SetActive(false);

            if (btnLookUp) btnLookUp.gameObject.SetActive(true);
            if (btnRestore) btnRestore.gameObject.SetActive(false);
        }
    }

    private void ShowUI()
    {
        if (!zoneUI) return;
        zoneUI.alpha = 1;
        zoneUI.interactable = true;
        zoneUI.blocksRaycasts = true;
    }

    private void HideUI()
    {
        if (!zoneUI) return;
        zoneUI.alpha = 0;
        zoneUI.interactable = false;
        zoneUI.blocksRaycasts = false;
    }
}
