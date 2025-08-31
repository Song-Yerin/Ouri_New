using UnityEngine;

public class ParabolaMover : MonoBehaviour
{
    [Header("옵션")]
    public bool rotateToVelocity = true;
    public GameObject impactVFX;
    public bool destroyOnFinish = false;

    Vector3 p0, v0, g;
    float T, t;
    bool active;

    /// <summary>
    /// 현재 위치 → target을 flightTime초에 도달하도록 포물선으로 이동
    /// </summary>
    public void LaunchTo(Vector3 target, float flightTime, Vector3? customGravity = null)
    {
        p0 = transform.position;
        T = Mathf.Max(0.01f, flightTime);
        g = customGravity ?? Physics.gravity;

        // v0 = (target - p0 - 0.5*g*T^2)/T  → 수평/수직 모두 포함
        v0 = (target - p0 - 0.5f * g * T * T) / T;

        Begin();
    }

    /// <summary>
    /// 타깃의 XZ와 착지 높이(targetY)를 지정.
    /// 수평 속도 크기(horizontalSpeed)로 달릴 때 자동으로 T를 계산해 포물선 이동
    /// </summary>
    public void LaunchToXZ(Vector3 targetXZ, float targetY, float horizontalSpeed, Vector3? customGravity = null)
    {
        p0 = transform.position;
        g = customGravity ?? Physics.gravity;

        Vector3 deltaXZ = new Vector3(targetXZ.x - p0.x, 0f, targetXZ.z - p0.z);
        float distXZ = deltaXZ.magnitude;
        horizontalSpeed = Mathf.Max(0.01f, horizontalSpeed);

        // 수평 시간 T = 거리 / 속도
        T = Mathf.Max(0.01f, distXZ / horizontalSpeed);

        // 수평 초기속도
        Vector3 v0xz = distXZ > 1e-4f ? deltaXZ.normalized * horizontalSpeed : Vector3.zero;
        // 수직 초기속도 (p(T) = p0 + v0*T + 0.5*g*T^2)
        float vy = (targetY - p0.y - 0.5f * g.y * T * T) / T;

        v0 = new Vector3(v0xz.x, vy, v0xz.z);

        Begin();
    }

    void Begin()
    {
        t = 0f;
        active = true;
        enabled = true;
    }

    void Update()
    {
        if (!active) return;

        t += Time.deltaTime;
        float tt = Mathf.Min(t, T);

        // p(t) = p0 + v0*t + 0.5*g*t^2
        Vector3 pos = p0 + v0 * tt + 0.5f * g * tt * tt;
        transform.position = pos;

        if (rotateToVelocity)
        {
            Vector3 vel = v0 + g * tt;
            if (vel.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(vel.normalized, Vector3.up);
        }

        if (t >= T)
        {
            active = false;
            if (impactVFX) Instantiate(impactVFX, transform.position, Quaternion.identity);
            if (destroyOnFinish) Destroy(gameObject);
        }
    }
}

