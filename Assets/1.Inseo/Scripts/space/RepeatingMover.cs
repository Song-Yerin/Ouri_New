using UnityEngine;

public class RepeatingMover : MonoBehaviour
{
    // 이동 방향을 선택하기 위한 열거형(Enum)
    public enum MoveAxis
    {
        Horizontal, // X축 (좌우)
        Vertical,   // Y축 (상하)
        Forward     // Z축 (앞뒤)
    }

    [Header("이동 설정")]
    [Tooltip("오브젝트가 왕복 운동할 축을 선택합니다.")]
    public MoveAxis moveAxis = MoveAxis.Horizontal;

    [Tooltip("오브젝트가 이동할 총 거리입니다.")]
    public float moveRange = 5f;

    [Tooltip("오브젝트의 이동 속도입니다.")]
    public float moveSpeed = 2f;

    [Tooltip("움직임을 부드럽게 할지, 아니면 선형적으로 움직일지 선택합니다.")]
    public bool useSmoothing = true;

    // 비공개 변수
    private Vector3 startPosition;
    private float cycleOffset;

    void Start()
    {
        // 스크립트가 시작될 때의 초기 위치를 저장합니다.
        startPosition = transform.position;
        // 항상 같은 위치에서 시작하도록 사이클 오프셋을 랜덤으로 설정합니다.
        cycleOffset = Random.Range(0f, Mathf.PI * 2);
    }

    void Update()
    {
        // 시간이 지남에 따라 왕복 운동을 위한 값을 계산합니다.
        // Mathf.Sin 함수는 -1과 1 사이를 부드럽게 왕복하는 값을 반환합니다.
        // Mathf.PingPong 함수는 0과 1 사이를 선형적으로 왕복하는 값을 반환합니다.
        float cycle;
        if (useSmoothing)
        {
            cycle = Mathf.Sin((Time.time + cycleOffset) * moveSpeed); // -1 ~ 1 사이를 부드럽게 왕복
        }
        else
        {
            cycle = Mathf.PingPong(Time.time * moveSpeed, 1f) * 2f - 1f; // -1 ~ 1 사이를 선형적으로 왕복
        }

        // 이동할 방향 벡터를 결정합니다.
        Vector3 directionVector;
        switch (moveAxis)
        {
            case MoveAxis.Vertical:
                directionVector = Vector3.up;
                break;
            case MoveAxis.Forward:
                directionVector = Vector3.forward;
                break;
            case MoveAxis.Horizontal:
            default:
                directionVector = Vector3.right;
                break;
        }

        // 초기 위치에서 계산된 오프셋만큼 떨어진 새로운 위치를 계산합니다.
        // moveRange의 절반만큼 양쪽으로 움직입니다.
        Vector3 offset = directionVector * cycle * (moveRange / 2f);
        transform.position = startPosition + offset;
    }

    // Scene 뷰에서 이동 경로를 시각적으로 보여주는 Gizmo 기능입니다.
    private void OnDrawGizmosSelected()
    {
        // 에디터가 플레이 모드가 아닐 때만 시작 위치를 업데이트합니다.
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
        }

        Vector3 directionVector;
        switch (moveAxis)
        {
            case MoveAxis.Vertical:
                directionVector = Vector3.up;
                break;
            case MoveAxis.Forward:
                directionVector = Vector3.forward;
                break;
            case MoveAxis.Horizontal:
            default:
                directionVector = Vector3.right;
                break;
        }

        Vector3 startPoint = startPosition - directionVector * (moveRange / 2f);
        Vector3 endPoint = startPosition + directionVector * (moveRange / 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPoint, endPoint);
        Gizmos.DrawSphere(startPoint, 0.15f);
        Gizmos.DrawSphere(endPoint, 0.15f);
    }
}
