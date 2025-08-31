using Controller;
using UnityEngine;

public class IdleManager : MonoBehaviour
{
    private Animator animator;
    private CreatureMover creatureMover;
    public float idleChangeTimeMin = 5f; // 최소 대기 시간
    public float idleChangeTimeMax = 10f; // 최대 대기 시간
    private float timer;
    private int idleCount = 2; // idle 애니메이션 개수 (idle1, idle2, idle3)

    void Start()
    {
        animator = GetComponent<Animator>();
        creatureMover = GetComponent<CreatureMover>();
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ChangeIdleAnimation();
            ResetTimer();
        }
    }

    void ChangeIdleAnimation()
    {
        // 70% 확률로 기본 Idle(0), 30% 확률로 나머지 Idle 중 하나 재생
        int randomIndex = 0;
        if (Random.value > 0.7f&&creatureMover.m_IsRun==false)
        {
            animator.SetTrigger("PlayIdle2");
        }

    }

    void ResetTimer()
    {
        timer = Random.Range(idleChangeTimeMin, idleChangeTimeMax);
    }
}
