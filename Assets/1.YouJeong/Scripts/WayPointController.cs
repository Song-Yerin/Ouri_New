using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SplinePath : MonoBehaviour
{
    [Tooltip("경로 웨이포인트들(순서대로!!)")]
    public List<Transform> waypoints = new();

    [Range(8, 1024)]
    public int samplesPerSegment = 64;

    private List<Vector3> _samples = new();
    private List<float> _cumLen = new();
    private float _totalLen;
    public float TotalLength => _totalLen;

    private void OnEnable() => Rebuild();
    private void OnValidate() => Rebuild();

    public void Rebuild()
    {
        _samples.Clear();
        _cumLen.Clear();
        _totalLen = 0f;

        if (waypoints.Count < 2) return;

        Vector3 prev = EvaluateRaw(0f);
        _samples.Add(prev);
        _cumLen.Add(0f);

        int segs = waypoints.Count - 1;
        int totalSamp = segs * samplesPerSegment;
        for (int i = 1; i <= totalSamp; i++)
        {
            float t = (float)i / totalSamp;
            Vector3 p = EvaluateRaw(t);
            _totalLen += Vector3.Distance(prev, p);
            _samples.Add(p);
            _cumLen.Add(_totalLen);
            prev = p;
        }
    }

    public Vector3 Evaluate(float t) => EvaluateRaw(Mathf.Clamp01(t));

    public Vector3 EvaluateTangent(float t)
    {
        float dt = 0.001f;
        Vector3 p1 = Evaluate(Mathf.Clamp01(t));
        Vector3 p2 = Evaluate(Mathf.Clamp01(t + dt));
        Vector3 v = p2 - p1;
        return v.sqrMagnitude > 1e-8f ? v.normalized : Vector3.forward;
    }

    public float DistanceToT(float s)
    {
        if (_samples.Count == 0) Rebuild();
        if (_totalLen <= 0f) return 0f;

        s = Mathf.Clamp(s, 0f, _totalLen);
        int lo = 0, hi = _cumLen.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (_cumLen[mid] < s) lo = mid + 1; else hi = mid;
        }
        int idx = Mathf.Clamp(lo, 1, _cumLen.Count - 1);
        float s0 = _cumLen[idx - 1];
        float s1 = _cumLen[idx];
        float u = (s1 > s0) ? (s - s0) / (s1 - s0) : 0f;

        float t0 = (idx - 1) / (float)(_cumLen.Count - 1);
        float t1 = (idx) / (float)(_cumLen.Count - 1);
        return Mathf.Lerp(t0, t1, u);
    }

    private Vector3 EvaluateRaw(float t)
    {
        int n = waypoints.Count;
        if (n == 0) return transform.position;
        if (n == 1) return waypoints[0].position;

        t = Mathf.Clamp01(t);
        float segT = t * (n - 1);
        int i = Mathf.FloorToInt(segT);
        float lt = segT - i;

        int i0 = Mathf.Clamp(i - 1, 0, n - 1);
        int i1 = Mathf.Clamp(i, 0, n - 1);
        int i2 = Mathf.Clamp(i + 1, 0, n - 1);
        int i3 = Mathf.Clamp(i + 2, 0, n - 1);

        Vector3 p0 = waypoints[i0].position;
        Vector3 p1 = waypoints[i1].position;
        Vector3 p2 = waypoints[i2].position;
        Vector3 p3 = waypoints[i3].position;

        return CatmullRom(p0, p1, p2, p3, lt);
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 +
            (-p0 + 3f*p1 - 3f*p2 + p3) * t3
        );
    }
}

