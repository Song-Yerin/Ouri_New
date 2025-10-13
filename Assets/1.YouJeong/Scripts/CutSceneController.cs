using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutSceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;  

    private void OnEnable()
    {

        playableDirector.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {

        playableDirector.stopped -= OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        
        SceneManager.LoadScene("SpaceShipLauncher");  
    }
}
