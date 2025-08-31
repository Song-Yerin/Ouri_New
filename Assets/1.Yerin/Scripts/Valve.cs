using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]   
public class Valve : MonoBehaviour
{
    // Inspector 
    [Header("Valve Info")]
    public int valveIndex;                    
    public Pipe pipe;                         
    public PipePuzzleManager puzzleManager;  

    [Header("Interaction")]
    public float rotateDuration = 1f;
    public KeyCode triggerKey = KeyCode.R;
    public string playerTag = "Player";

    
    private bool isPlayerNear = false;
    private Tween spinTween;

   
    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(triggerKey))
        {
            RotateOnce();                         // 손잡이 회전 애니메이션

            // ① 기본 가스 FX 재생 
            if (pipe)
            {
                pipe.PlayTurnFX();
            }
            else
            {
                Debug.LogWarning($"{name} : Pipe 참조가 비었습니다.", this);
            }

            // ② 퍼즐 순서 검사 
            if (puzzleManager)
            {
                puzzleManager.CheckValve(this);
            }
            else
            {
                Debug.LogWarning($"{name} : PuzzleManager 참조가 비었습니다.", this);
            }
        }
    }

    // 손잡이 360° 회전
    private void RotateOnce()
    {
        spinTween?.Kill();
        spinTween = transform
            .DORotate(new Vector3(0, 360, 0), rotateDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear);
    }

    // 근접 판정 
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            isPlayerNear = false;
    }

    void OnDestroy() => spinTween?.Kill();
}
