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

    //public PlayableDirector cutsceneDirector; // Timeline ��Ʈ�ѷ�
    public VideoPlayer cutsceneVideoPlayer; //VideoPlayer ����

    float cutsceneDuration = 5f; // �ƾ��� ���� �� Enemy�� �̵��� �����ϱ� �� ��� �ð�

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
        Debug.Log("�ƾ� ����");
        StartCoroutine(StartCutscene());
      
    }
    private IEnumerator StartCutscene()
    {
        Debug.Log("�ƾ� ���");

        //// �ƾ� ��� (Timeline)
        //if (cutsceneDirector != null)
        //{
        //    cutsceneDirector.Play();
        //    // Timeline�� ���̸� ��ٸ��ų� ������ cutsceneDuration ���
        //    yield return new WaitForSeconds((float)cutsceneDirector.duration);
        //}
        //else
        //{
        //    Debug.LogWarning("cutsceneDirector�� �Ҵ���� ����");
        //    yield return new WaitForSeconds(cutsceneDuration);
        //}

        //Debug.Log("�ƾ� ����, Enemy �̵� ����");
        if (cutsceneVideoPlayer != null)
        {

            cutsceneVideoPlayer.Play();

            // ���� ���� ������ ���
            while (cutsceneVideoPlayer.isPlaying)
            {
                yield return null;
            }
        }

        cutsceneVideoPlayer.Stop();                    // ���� ���߰�
        cutsceneVideoPlayer.enabled = false;           // ���� �÷��̾� ��Ȱ��ȭ

        StartEnemyAction();
    }
    private void StartEnemyAction()
    {
        Debug.Log("�� anim bool ����");
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

    public void StopChase() // ������ ���
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
        // �ʱ�ȭ ����
        setDestinationModule.Chasing = false;
        // ��ġ �ʱ�ȭ
        this.transform.position = playerResetPosition.position;
        enemy.transform.position = enemyResetPosition.position;
    }
    public IEnumerator Reset()
    {

        // ���
        yield return new WaitForSeconds(2f);

        //���� �簳
        setDestinationModule.Chasing = true;
        setDestinationModule.ResetAnim();

    }
}
