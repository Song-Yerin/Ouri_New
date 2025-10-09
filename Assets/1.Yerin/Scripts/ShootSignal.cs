using UnityEngine;

public class ShootSignal : MonoBehaviour
{
    public enum ShootMode { AddBy, MoveTo }

    [Header("Motion")]
    public ShootMode mode = ShootMode.AddBy;  // AddBy: 현재에서 +amount, MoveTo: targetY로 이동
    public float amountOrTargetY = 2f;        // AddBy면 ‘올릴 양’, MoveTo면 ‘목표 Y’
    public float duration = 1f;               // 이동 시간(초)
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useLocal = false;             // 로컬/월드 좌표 선택
    public bool interruptAndRestart = true;   // 진행 중 신호 재수신 시 다시 시작할지

    [Header("Optional")]
    public bool clampBelow = false;           // MoveTo 일 때만: 현재보다 낮게 떨어지지 않게
    public bool snapAtEnd = true;             // float 오차 방지용 스냅

    Coroutine _running;

    // 타임라인 SignalReceiver에서 이 메서드를 호출하면 됨
    public void OnSignalShoot()
    {
        if (interruptAndRestart && _running != null) StopCoroutine(_running);
        if (_running == null) _running = StartCoroutine(ClimbRoutine());
    }

    System.Collections.IEnumerator ClimbRoutine()
    {
        Vector3 start = useLocal ? transform.localPosition : transform.position;
        Vector3 end = start;

        if (mode == ShootMode.AddBy)
        {
            end.y = start.y + amountOrTargetY;
        }
        else // MoveTo
        {
            float target = amountOrTargetY;
            if (clampBelow && target < start.y) target = start.y;
            end.y = target;
        }

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = easing.Evaluate(Mathf.Clamp01(t));
            Vector3 p = Vector3.LerpUnclamped(start, end, k);
            if (useLocal) transform.localPosition = p;
            else transform.position = p;
            yield return null;
        }

        if (snapAtEnd)
        {
            if (useLocal) transform.localPosition = end;
            else transform.position = end;
        }

        _running = null;
    }

    // 필요하면 되돌리는 신호도 쓸 수 있게
    public void OnSignalResetY(float targetY)
    {
        amountOrTargetY = targetY;
        mode = ShootMode.MoveTo;
        OnSignalShoot();
    }
}
