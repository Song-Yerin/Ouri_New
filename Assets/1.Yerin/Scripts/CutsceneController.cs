using Controller;
using UnityEngine;

[RequireComponent(typeof(CreatureMover))]
[RequireComponent(typeof(AutoWalk))]
public class CutsceneController : MonoBehaviour
{
    private CreatureMover mover;
    private AutoWalk auto;

    void Awake()
    {
        mover = GetComponent<CreatureMover>();
        auto = GetComponent<AutoWalk>();

        // ��ҿ��� ���� ���� �� AutoWalk OFF
        mover.enabled = true;
        auto.enabled = false;
    }

    // ���� Ÿ�Ӷ��� Signal���� ȣ�� ��������������������������������������
    public void BeginCutscene()   // AutoWalk ON
    {
        mover.enabled = false;
        auto.enabled = true;
    }

    public void EndCutscene()     // ���� ���� ����
    {
        auto.enabled = false;
        mover.enabled = true;
    }
}

