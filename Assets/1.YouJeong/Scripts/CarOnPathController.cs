using UnityEngine;

public class CarOnPathController : MonoBehaviour, IPoolable
{
    public SplinePath path;
    public float speed = 10f;          // m/s
    public float lookAhead = 0.5f;     // 회전 예측 거리(미터)
    public float endMargin = 0.2f;     // 끝에서 약간 남기고 반환

    private float _distance;
    private bool _moving;

    public void Init(SplinePath p, float startOffset, float speedMps)
    {
        path = p;
        speed = speedMps;
        path.Rebuild();

        _distance = Mathf.Clamp(startOffset, 0f, path.TotalLength);
        var t = path.DistanceToT(_distance);
        transform.position = path.Evaluate(t);
        AlignToPath(t);
        _moving = true;
    }

    private void Update()
    {
        if (!_moving || path == null || path.TotalLength <= 0f) return;

        _distance += speed * Time.deltaTime;

        float endDist = Mathf.Max(0f, path.TotalLength - endMargin);
        if (_distance >= endDist)
        {
            _moving = false;
            MultiPrefabPoolManager.Instance.Despawn(gameObject);
            return;
        }

        float t = path.DistanceToT(_distance);
        transform.position = path.Evaluate(t);
        AlignToPath(t);
    }

    private void AlignToPath(float t)
    {
        float dt = (lookAhead <= 0f || path.TotalLength <= 0f) ? 0f : lookAhead / path.TotalLength;
        float t2 = Mathf.Clamp01(t + dt);
        Vector3 dir = path.EvaluateTangent(t2);
        if (dir.sqrMagnitude > 1e-6f)
        {
            var target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 12f * Time.deltaTime);
        }
    }

    public void OnSpawned()
    {
        _moving = true;
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
    }

    public void OnDespawned() => _moving = false;
}

