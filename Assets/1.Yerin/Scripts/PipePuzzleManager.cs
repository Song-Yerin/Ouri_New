using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables; // 꼭 필요!



public class PipePuzzleManager : MonoBehaviour
{
    // 성공 컷씬
    public PlayableDirector successCutscene; 

    // 정답 시퀀스 
    private readonly int[] sequence = { 0, 1, 3, 2, 1, 2 };

    // 성공 FX-쌍 
    private readonly Dictionary<(int, int), int> pairToPipe = new()
    {
        { (0,1), 0 }, { (3,2), 3 }, { (2,1), 1 }, { (1,2), 2 }
    };

    // 진행 상태
    private int step = 0;        // 현재 sequence 
    private int lastValve = -1;       // 직전 입력
    private readonly HashSet<int> activated = new();
    private bool puzzleCleared = false;    // 통과 후 재도전 금지

    // Inspector 
    public List<Valve> valves;  
    public List<Pipe> pipes;    

    void Awake() { foreach (var v in valves) v.puzzleManager = this; }

    // Valve가 호출
    public void CheckValve(Valve clicked)
    {
        if (puzzleCleared) return;

        int cur = clicked.valveIndex;

        // 현재 입력이 시퀀스 기대값과 일치? 
        if (cur == sequence[step])
        {
            // (prev,cur)가 FX 짝꿍이면 해당 파이프 성공 FX ON 
            if (step > 0 &&
                pairToPipe.TryGetValue((sequence[step - 1], cur), out int pipeIdx) &&
                activated.Add(pipeIdx))
            {
                pipes[pipeIdx].PlaySuccessFX();
            }

            step++;
            lastValve = cur;

            // 정답 시퀀스 완주 → 퍼즐 통과 
            if (step >= sequence.Length)
            {
                puzzleCleared = true;
                Debug.Log("퍼즐 완료!");

                // 컷씬 재생
                if (successCutscene != null)
                {
                    successCutscene.Play();
                }


            }
            return;
        }

        // 시퀀스에서 벗어났음 → 성공 FX만 리셋
        ResetAllFXExcept(cur);

        // 현재 입력이 시퀀스 첫 값(0)이면 곧바로 새 시도 1단계로 진입 
        if (cur == sequence[0])
        {
            step = 1;
            lastValve = cur;
        }
    }

    // 전체 FX OFF + 진행 변수 초기화 
    private void ResetAllFX()
    {
        foreach (var p in pipes) p.ResetFX();

        activated.Clear();
        step = 0;
        lastValve = -1;
        Debug.Log("초기화: 모든 성공 FX OFF");
    }

    // 성공 FX OFF + 진행 변수 초기화 
    private void ResetAllFXExcept(int wrongValveIndex)
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].ResetFX();

            // 잘못 누른 현재 valve의 기본 가스만 다시 켬
            if (valves[i].valveIndex == wrongValveIndex)
                pipes[i].PlayTurnFXOnly();
        }

        activated.Clear();
        step = 0;
        lastValve = -1;
        Debug.Log($"잘못된 입력 ➜ Valve {wrongValveIndex}, 해당 기본 가스 FX만 다시 ON");
    }
}
