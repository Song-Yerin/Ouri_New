using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class SetDestinationModule : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Vector3 destination;

    public bool Chasing = false;
    private float stoppingDistance = 0.1f;
    [SerializeField] float moveSpeed = 2f;

    public bool isFailed = false;
    public bool isSuccess = false;
    Animator anim;


    [SerializeField] StartChasingModule startChasingModule;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isSuccess)
            return;

        if (isFailed)
        {
            enemyReset();
        }

        if (Chasing)
        {

            float distance = Vector3.Distance(transform.position, destination);

            if (distance > stoppingDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    moveSpeed * Time.deltaTime
                );
            }
            else
            {
                // 도착했다고 판단
                Debug.Log("목적지 근처에 도달함");

                //애니메이션 정
                if (anim != null)
                    anim.SetBool("isRunning", false);

                Chasing = false;
                isFailed = true;
            }
        }
    }

    public void SetDestination(Vector3 vector3)
    {
        destination = vector3;

    }
    public void enemyReset()
    {
        startChasingModule.ResetPosition();
        StartCoroutine(startChasingModule.Reset());
        isFailed = false;
    }

    public void ResetAnim()
    {
        anim.SetBool("isRunning", true);
    }
}
