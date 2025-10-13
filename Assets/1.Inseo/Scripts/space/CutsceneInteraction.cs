using UnityEngine;
using UnityEngine.Playables; // 컷씬(Timeline)을 제어하기 위해 꼭 필요합니다.

// 이 스크립트가 붙은 오브젝트에는 반드시 Collider 컴포넌트가 있어야 합니다.
[RequireComponent(typeof(Collider))]
public class CutsceneInteraction : MonoBehaviour
{
    [Header("필수 설정")]
    [Tooltip("상호작용 시 재생될 컷씬(PlayableDirector)을 연결합니다.")]
    public PlayableDirector cutsceneToPlay;

    [Header("UI 피드백 (선택 사항)")]
    [Tooltip("상호작용이 가능할 때 표시할 UI 오브젝트 (예: 'E 키를 누르세요')")]
    public GameObject interactionPromptUI;

    // 내부 상태 변수
    private bool isPlayerInZone = false;

    private void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 콜라이더를 반드시 트리거로 설정합니다.
        GetComponent<Collider>().isTrigger = true;

        // UI가 연결되어 있다면, 게임 시작 시에는 우선 숨겨둡니다.
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 영역에 들어온 오브젝트가 'Player' 태그를 가졌는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;

            // 상호작용 UI가 있다면 활성화하여 플레이어에게 알립니다.
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 영역에서 나간 오브젝트가 'Player' 태그를 가졌는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;

            // 플레이어가 영역을 벗어나면 상호작용 UI를 숨깁니다.
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // 플레이어가 영역 안에 있고, 'E' 키를 눌렀을 때만 실행됩니다.
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            // 재생할 컷씬이 Inspector 창에 할당되었는지 확인합니다.
            if (cutsceneToPlay != null)
            {
                Debug.Log("E키 입력: 컷씬(" + cutsceneToPlay.name + ")을 재생합니다.");

                // 컷씬을 재생합니다.
                cutsceneToPlay.Play();

                // 컷씬이 재생되면 더 이상 상호작용할 필요가 없으므로 UI를 숨깁니다.
                if (interactionPromptUI != null)
                {
                    interactionPromptUI.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("재생할 컷씬이 할당되지 않았습니다!", this.gameObject);
            }
        }
    }
}
