using UnityEngine;

public class RotateHalfOnSignal : MonoBehaviour
{
    public float duration = 1f; // 몇 초 동안 회전할지
    public bool clockwise = true; // 시계 방향 여부
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine _running;

    public void OnSignalRotateHalf()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(RotateRoutine());
    }

    System.Collections.IEnumerator RotateRoutine()
    {
        float startRotY = transform.eulerAngles.y;
        float endRotY = startRotY + (clockwise ? 180f : -180f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = easing.Evaluate(Mathf.Clamp01(t));
            float currentY = Mathf.LerpAngle(startRotY, endRotY, k);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, currentY, transform.eulerAngles.z);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, endRotY, transform.eulerAngles.z);
        _running = null;
    }
}

