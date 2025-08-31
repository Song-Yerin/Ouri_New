using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.UIElements.Experimental;
using UnityEngine.Video;

using System.Collections;
using System.Collections.Generic;
public class StartChasingModule : MonoBehaviour
{
    //// Start is called before the first frame update

    bool hasTriggered = false;
    bool startChase = false;

    //public PlayableDirector cutsceneDirector; // Timeline 컨트롤러
    public VideoPlayer cutsceneVideoPlayer; //VideoPlayer 참조

    float cutsceneDuration = 5f; // 컷씬이 끝난 후 Enemy가 이동을 시작하기 전 대기 시간

    public GameObject enemy;
    public Transform player;
    public string enemyAnimationTrigger = "isRunning";

    SetDestinationModule setDestinationModule;

    public Transform playerResetPosition;
    public Transform enemyResetPosition;

    void Start()
    {
        setDestinationModule = enemy.GetComponent<SetDestinationModule>();
    }

    // Update is called once per frame
    void Update()
    {
        if(startChase)
        {
            EnemyChase();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        Debug.Log("1");
     
        hasTriggered = true;
        Debug.Log("컷씬 시작");
        StartCoroutine(StartCutscene());
      
    }
    private IEnumerator StartCutscene()
    {
        Debug.Log("컷씬 재생");

        //// 컷씬 재생 (Timeline)
        //if (cutsceneDirector != null)
        //{
        //    cutsceneDirector.Play();
        //    // Timeline의 길이를 기다리거나 지정된 cutsceneDuration 사용
        //    yield return new WaitForSeconds((float)cutsceneDirector.duration);
        //}
        //else
        //{
        //    Debug.LogWarning("cutsceneDirector가 할당되지 않음");
        //    yield return new WaitForSeconds(cutsceneDuration);
        //}

        //Debug.Log("컷씬 종료, Enemy 이동 시작");
        if (cutsceneVideoPlayer != null)
        {

            cutsceneVideoPlayer.Play();

            // 영상 끝날 때까지 대기
            while (cutsceneVideoPlayer.isPlaying)
            {
                yield return null;
            }
        }

        cutsceneVideoPlayer.Stop();                    // 영상 멈추고
        cutsceneVideoPlayer.enabled = false;           // 영상 플레이어 비활성화

        StartEnemyAction();
    }
    private void StartEnemyAction()
    {
        Debug.Log("적 anim bool 시작");
        if (enemy == null || player == null) return;

        Animator anim = enemy.GetComponent<Animator>();

        if (anim != null)
        {
            anim.SetBool(enemyAnimationTrigger,true);
        }
        startChase = true;
        setDestinationModule.Chasing = true;
    }
    
    private void EnemyChase()
    {
        if (enemy != null)
        {
            setDestinationModule.SetDestination(player.position);
        }

    }

    public void StopChase() // 성공의 경우
    {
        startChase = false;
        setDestinationModule.Chasing = false;
        setDestinationModule.isSuccess = true;

        Animator anim = enemy.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool(enemyAnimationTrigger, false);
        }
    }

    public void ResetPosition()
    {
        // 초기화 로직
        setDestinationModule.Chasing = false;
        // 위치 초기화
        this.transform.position = playerResetPosition.position;
        enemy.transform.position = enemyResetPosition.position;
    }
    public IEnumerator Reset()
    {

        // 대기
        yield return new WaitForSeconds(2f);

        //게임 재개
        setDestinationModule.Chasing = true;
        setDestinationModule.ResetAnim();

    }
}
