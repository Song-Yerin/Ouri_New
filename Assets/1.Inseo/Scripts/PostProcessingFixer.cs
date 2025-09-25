using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class GlobalVolumeSceneFixer : MonoBehaviour
{
    [Header("씬 로드 시 Global Volume 자동 복구")]
    [Tooltip("이 스크립트가 씬 시작 시 자동으로 복구 로직을 실행할지 여부입니다.")]
    [SerializeField] private bool fixOnStart = true;

    void Start()
    {
        if (fixOnStart)
        {
            StartCoroutine(FixPostProcessingRoutine());
        }
    }

    private IEnumerator FixPostProcessingRoutine()
    {
        // 씬의 모든 오브젝트가 로드되고 첫 프레임 렌더링이 끝날 때까지 대기
        yield return new WaitForEndOfFrame();

        Debug.Log("포스트 프로세싱 복구를 시작합니다...");

        // 1. 메인 카메라의 Post Processing 설정을 강제로 켭니다.
        ForceEnableCameraPostProcessing();

        // 2. 현재 씬의 Global Volume을 찾아서 강제로 새로고침합니다.
        yield return RefreshGlobalVolume();

        Debug.Log("포스트 프로세싱 복구가 완료되었습니다.");
    }

    private void ForceEnableCameraPostProcessing()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("씬에 'MainCamera' 태그가 지정된 카메라가 없습니다. 복구를 진행할 수 없습니다.");
            return;
        }

        // --- [오류 수정 부분] ---
        // 카메라의 URP 추가 데이터 가져오기 (GetComponent로 수정)
        var cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData != null)
        {
            // Post Processing 옵션을 스크립트로 다시 활성화
            cameraData.renderPostProcessing = true;
            Debug.Log($"카메라 '{mainCamera.name}'의 Post Processing을 강제로 활성화했습니다.");
        }
        else
        {
            Debug.LogWarning($"카메라 '{mainCamera.name}'에 UniversalAdditionalCameraData 컴포넌트가 없습니다! URP 설정이 올바른지 확인하세요.");
        }
    }

    private IEnumerator RefreshGlobalVolume()
    {
        // 현재 씬에 있는 모든 Volume 컴포넌트를 찾습니다.
        Volume[] volumes = FindObjectsOfType<Volume>();
        bool foundGlobalVolume = false;

        foreach (Volume vol in volumes)
        {
            // Global Volume일 경우에만 새로고침 진행
            if (vol.isGlobal)
            {
                foundGlobalVolume = true;
                vol.enabled = false;
                yield return null; // 한 프레임 대기
                vol.enabled = true;
                Debug.Log($"'{vol.gameObject.name}'에 있는 Global Volume을 새로고침했습니다.");
            }
        }

        if (!foundGlobalVolume)
        {
            Debug.LogWarning("현재 씬에서 Global Volume을 찾지 못했습니다.");
        }
    }
}
