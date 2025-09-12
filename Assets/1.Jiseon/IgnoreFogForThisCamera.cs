using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IgnoreFogForThisCamera : MonoBehaviour
{
    private bool fogBackup;  // 원래 포그 상태 저장용

    void OnPreRender()
    {
        // 원래 포그 상태 저장
        fogBackup = RenderSettings.fog;

        // 이 카메라 렌더 시작 전에 포그 끄기
        RenderSettings.fog = false;
    }

    void OnPostRender()
    {
        // 렌더 끝난 후 포그 상태 원복
        RenderSettings.fog = fogBackup;
    }

    void OnDisable()
    {
        // 혹시 카메라 비활성화 시 포그가 꺼진 채로 남지 않게 원복 보장
        RenderSettings.fog = fogBackup;
    }
}
