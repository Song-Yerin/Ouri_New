using UnityEngine;

[System.Serializable]
public class LanePrefabOption
{
    public CarPoolTag tag;                 // string → enum
    [Range(0f, 1f)] public float weight = 1f;
}

[System.Serializable]
public class RoadLane
{
    public SplinePath path;                // 레인 경로 (직선+곡선 혼합)
    public Vector2 speedRange = new Vector2(8f, 14f);          // new() → 명시 생성
    public LanePrefabOption[] prefabOptions;                   // 스폰 허용 프리팹(풀 태그)
    public float spawnInterval = 1.2f;
    public int maxActiveOnLane = 12;

    [HideInInspector] public float timer;
    [HideInInspector] public int activeCount;
}

public class MultiLaneRoadSpawner : MonoBehaviour
{
    public RoadLane[] lanes;

    private void Update()
    {
        if (lanes == null) return;

        foreach (var lane in lanes)
        {
            if (lane == null || lane.path == null) continue;

            lane.timer += Time.deltaTime;
            if (lane.timer < lane.spawnInterval) continue;
            if (lane.activeCount >= lane.maxActiveOnLane) { lane.timer = 0f; continue; }

            lane.timer = 0f;

            // 프리팹 태그(=풀 태그) 가중치 선택
            CarPoolTag tag = PickTag(lane.prefabOptions);

            // 시작 위치/회전
            lane.path.Rebuild();
            if (lane.path.TotalLength <= 0f) continue;

            float t0 = 0f;
            Vector3 p0 = lane.path.Evaluate(t0);
            Vector3 fwd = lane.path.EvaluateTangent(t0);
            Quaternion rot = fwd.sqrMagnitude > 0.001f ? Quaternion.LookRotation(fwd, Vector3.up) : Quaternion.identity;

            // 풀에서 스폰 (enum 사용)
            var go = MultiPrefabPoolManager.Instance.Spawn(tag, p0, rot);
            if (!go) continue;

            // 이동 컨트롤러 세팅
            var car = go.GetComponent<CarOnPathController>() ?? go.AddComponent<CarOnPathController>();
            float speed = Random.Range(lane.speedRange.x, lane.speedRange.y);
            car.Init(lane.path, 0f, speed);

            // 레인 활성 개수 관리
            lane.activeCount++;
            var hook = go.GetComponent<LaneDespawnHook>() ?? go.AddComponent<LaneDespawnHook>();
            hook.Bind(() => lane.activeCount = Mathf.Max(0, lane.activeCount - 1));
        }
    }

    private CarPoolTag PickTag(LanePrefabOption[] options)
    {
        // 옵션이 비었으면 기본값 반환(0번째 enum 값)
        if (options == null || options.Length == 0) return default;

        float total = 0f;
        foreach (var o in options) total += Mathf.Max(0f, o.weight);
        if (total <= 0f) return options[Random.Range(0, options.Length)].tag;

        float r = Random.value * total;
        float acc = 0f;
        foreach (var o in options)
        {
            acc += Mathf.Max(0f, o.weight);
            if (r <= acc) return o.tag;
        }
        return options[options.Length - 1].tag; // ^1 대신 호환 가능한 방식
    }
}

public class LaneDespawnHook : MonoBehaviour, IPoolable
{
    private System.Action _onDespawn;
    public void Bind(System.Action cb) => _onDespawn = cb;
    public void OnSpawned() { }
    public void OnDespawned() => _onDespawn?.Invoke();
}

