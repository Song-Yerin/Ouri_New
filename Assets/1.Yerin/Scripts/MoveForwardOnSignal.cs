using UnityEngine;

public class MoveForwardOnSignal : MonoBehaviour
{
    public float distance = 2f; // 이동 거리
    public float duration = 1f; // 이동 시간
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useLocal = true; // 로컬 방향 기준

    Coroutine _running;

    public void OnSignalMoveForward()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(MoveRoutine());
    }

    System.Collections.IEnumerator MoveRoutine()
    {
        Vector3 start = transform.position;
        Vector3 dir = useLocal ? transform.forward : Vector3.forward;
        Vector3 end = start + dir * distance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = easing.Evaluate(Mathf.Clamp01(t));
            transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        transform.position = end;
        _running = null;
    }
}
