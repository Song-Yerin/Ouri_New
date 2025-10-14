using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class jumpMapCutScene : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private PlayableDirector playableDirector2;

    private void OnEnable()
    {

        playableDirector.stopped += OnTimelineStopped;
        playableDirector2.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {

        playableDirector.stopped -= OnTimelineStopped;
        playableDirector2.stopped -= OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {

        this.gameObject.SetActive(false);
    }
}
