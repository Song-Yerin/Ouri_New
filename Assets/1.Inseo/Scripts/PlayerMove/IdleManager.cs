using Controller;
using UnityEngine;

public class IdleManager : MonoBehaviour
{
    private Animator animator;
    private CreatureMover creatureMover;
    public float idleChangeTimeMin = 5f; // �ּ� ��� �ð�
    public float idleChangeTimeMax = 10f; // �ִ� ��� �ð�
    private float timer;
    private int idleCount = 2; // idle �ִϸ��̼� ���� (idle1, idle2, idle3)

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
        // 70% Ȯ���� �⺻ Idle(0), 30% Ȯ���� ������ Idle �� �ϳ� ���
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
