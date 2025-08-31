using UnityEngine;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    [Header("기본 가스 FX ")]
    [SerializeField] private List<GameObject> turnFxRoots = new();

    [Header("성공 시 켜질 FX 루트")]
    [SerializeField] private List<GameObject> successFxRoots = new();


    // 최초 한 번: 다 꺼 두기
    void Awake() => ResetFX();


    // 밸브를 돌릴 때마다 호출
    public void PlayTurnFX()
    {
        foreach (var go in turnFxRoots)
        {
            if (!go)
            {
                Debug.LogWarning($"{name} : turnFxRoots 항목이 비었습니다", this);
                continue;
            }

            Debug.Log($"켜는 중 ➜ {go.name}", go);
            go.SetActive(true);          // 조건 없이 ON
        }
    }

    // 짝꿍 성공 시 호출 
    public void PlaySuccessFX()
    {
        // 기본 가스 FX 전부 OFF 
        foreach (var go in turnFxRoots)
            if (go && go.activeSelf) go.SetActive(false);

        // 성공 FX 전부 ON  
        foreach (var go in successFxRoots)
            if (go && !go.activeSelf) go.SetActive(true);
    }

    // 퍼즐 리셋 / 시작 시 호출 
    public void ResetFX()
    {
        foreach (var go in turnFxRoots) if (go) go.SetActive(false);
        foreach (var go in successFxRoots) if (go) go.SetActive(false);
    }

    // 시퀸스 실패 시 트는 기본 가스 FX
    public void PlayTurnFXOnly()
    {
        foreach (var go in turnFxRoots)
        {
            if (go && !go.activeSelf)
                go.SetActive(true);
        }
    }
}

