using Cinemachine;
using Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinemachineController : MonoBehaviour
{
    public PlayableDirector playableDirector; 
    public GameObject cinemachineBrain;
    public GameObject playerCam;
    public MovePlayerInput movePlayerInput;

    void Start()
    {
        cinemachineBrain.SetActive(true);
        if (playableDirector != null)
        {
            // Playable Director의 stopped 이벤트에 콜백 함수 추가
            playableDirector.stopped += OnCutsceneStopped;
        }
    }

    // 컷씬이 끝났을 때 호출되는 함수
    private void OnCutsceneStopped(PlayableDirector director)
    {
        if (cinemachineBrain != null)
        {
            // Cinemachine Brain 비활성화
            //playerCam.SetActive(true);
            cinemachineBrain.SetActive(false);
            //movePlayerInput.enabled = true;
        }
    }

    void OnDestroy()
    {
        // 이벤트에서 제거 (메모리 누수 방지)
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnCutsceneStopped;
        }
    }
}
