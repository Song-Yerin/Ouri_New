using System.Collections.Generic;
using UnityEngine;

public enum CarPoolTag
{
    BlueCar,
    RedCar,
    GrayCar,
    Truck,
    Bus,
    Taxi
}

[System.Serializable] // Inspector에서 보이도록
public class PoolEntry
{
    public CarPoolTag tag;      // 문자열 대신 enum
    public GameObject prefab;
    public int initialSize = 10;
    public bool expandable = true;
}

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

public class PoolMember : MonoBehaviour
{
    // 어떤 풀에서 나왔는지 기록 (반납할 때 사용)
    public CarPoolTag poolTag;   // ← string ▶ enum
}

public class MultiPrefabPoolManager : MonoBehaviour
{
    public static MultiPrefabPoolManager Instance { get; private set; }

    [SerializeField] private List<PoolEntry> poolsConfig = new List<PoolEntry>();

    // Unity 버전 호환을 위해 명시형 생성자 사용
    private readonly Dictionary<CarPoolTag, Queue<GameObject>> _pools =
        new Dictionary<CarPoolTag, Queue<GameObject>>();
    private readonly Dictionary<CarPoolTag, PoolEntry> _configByTag =
        new Dictionary<CarPoolTag, PoolEntry>();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildPools();
    }

    private void BuildPools()
    {
        _pools.Clear();
        _configByTag.Clear();

        foreach (var cfg in poolsConfig)
        {
            if (cfg == null || cfg.prefab == null) continue;

            var q = new Queue<GameObject>();
            for (int i = 0; i < Mathf.Max(0, cfg.initialSize); i++)
            {
                var go = Instantiate(cfg.prefab);
                go.SetActive(false);

                var mem = go.GetComponent<PoolMember>() ?? go.AddComponent<PoolMember>();
                mem.poolTag = cfg.tag; // enum 기록
                q.Enqueue(go);
            }

            _pools[cfg.tag] = q;
            _configByTag[cfg.tag] = cfg;
        }
    }

    public GameObject Spawn(CarPoolTag tag, Vector3 pos, Quaternion rot)
    {
        if (!_pools.ContainsKey(tag))
        {
            Debug.LogWarning($"[Pool] No pool with tag '{tag}'.");
            return null;
        }

        GameObject go = null;
        var q = _pools[tag];

        while (q.Count > 0 && go == null)
        {
            var candidate = q.Dequeue();
            if (candidate) go = candidate;
        }

        if (go == null)
        {
            var cfg = _configByTag[tag];
            if (!cfg.expandable) return null;

            go = Instantiate(cfg.prefab);
            var mem = go.GetComponent<PoolMember>() ?? go.AddComponent<PoolMember>();
            mem.poolTag = tag; // enum 기록
        }

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        foreach (var p in go.GetComponentsInChildren<IPoolable>(true))
            p.OnSpawned();

        return go;
    }

    public void Despawn(GameObject go)
    {
        if (!go) return;

        foreach (var p in go.GetComponentsInChildren<IPoolable>(true))
            p.OnDespawned();

        var mem = go.GetComponent<PoolMember>();
        if (!mem)
        {
            go.SetActive(false);
            Debug.LogWarning("[Pool] PoolMember missing. Disabled object but did not return to pool.");
            return;
        }

        go.SetActive(false);

        // enum 키로 동일 풀에 반환
        if (!_pools.ContainsKey(mem.poolTag))
            _pools[mem.poolTag] = new Queue<GameObject>();

        _pools[mem.poolTag].Enqueue(go);
    }
}


