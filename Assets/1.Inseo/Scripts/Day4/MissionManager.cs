using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    [Header("미션 설정")]
    [Tooltip("NavMesh 위에 스폰될 오브젝트 A의 프리팹")]
    public GameObject objectAPrefab;

    [Tooltip("미션 클리어에 필요한 총 점수")]
    public int goal = 10;

    [Tooltip("씬에 동시에 존재할 오브젝트의 최대 개수")]
    public int maxConcurrentObjects = 3;

    [Tooltip("오브젝트가 스폰될 영역을 지정하는 Collider")]
    public Collider spawnArea; // spawnRadius를 이 변수로 대체

    [Header("미션 상태")]
    [SerializeField]
    private int currentScore = 0;
    private bool isMissionActive = false;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void OnEnable()
    {
        StartMission();
    }

    private void StartMission()
    {
        if (isMissionActive) return;

        // 스폰 영역이 할당되었는지 먼저 확인
        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area Collider가 할당되지 않았습니다! MissionManager의 Inspector를 확인해주세요.");
            gameObject.SetActive(false); // 미션 비활성화
            return;
        }

        isMissionActive = true;
        currentScore = 0;
        spawnedObjects.Clear();
        Debug.Log("미션 시작!");

        SpawnInitialObjects();
    }

    private void SpawnInitialObjects()
    {
        for (int i = 0; i < maxConcurrentObjects; i++)
        {
            SpawnSingleObject();
        }
    }

    private void SpawnSingleObject()
    {
        if (objectAPrefab == null)
        {
            Debug.LogError("objectAPrefab이 할당되지 않았습니다!");
            return;
        }

        // 수정된 함수를 호출하여 스폰 위치를 가져옴
        Vector3 randomPosition = GetRandomNavMeshPositionInBounds();

        if (randomPosition != Vector3.zero)
        {
            GameObject spawnedObject = Instantiate(objectAPrefab, randomPosition, Quaternion.identity);

            var collectible = spawnedObject.GetComponent<CollectibleObject>();
            if (collectible == null) collectible = spawnedObject.AddComponent<CollectibleObject>();
            collectible.missionManager = this;

            spawnedObjects.Add(spawnedObject);
        }
        else
        {
            Debug.LogWarning("지정된 Spawn Area 내의 NavMesh 위에서 유효한 스폰 위치를 찾는 데 실패했습니다.");
        }
    }

    // 지정된 Collider 영역 내의 랜덤한 NavMesh 위치를 찾는 함수
    private Vector3 GetRandomNavMeshPositionInBounds()
    {
        Bounds spawnBounds = spawnArea.bounds;

        for (int i = 0; i < 30; i++) // 최대 30번 시도
        {
            // 콜라이더의 경계 내에서 랜덤한 X, Z 좌표를 구함
            float randomX = Random.Range(spawnBounds.min.x, spawnBounds.max.x);
            float randomZ = Random.Range(spawnBounds.min.z, spawnBounds.max.z);

            // 시작 높이는 콜라이더의 중심으로 설정
            Vector3 randomPoint = new Vector3(randomX, spawnBounds.center.y, randomZ);

            NavMeshHit navHit;
            // randomPoint에서 100f 반경 내의 NavMesh를 검색 (충분히 큰 값)
            if (NavMesh.SamplePosition(randomPoint, out navHit, 100f, NavMesh.AllAreas))
            {
                return navHit.position;
            }
        }
        return Vector3.zero; // 30번 시도 후에도 못 찾으면 실패
    }

    public void OnObjectCollected(GameObject collectedObject)
    {
        if (!isMissionActive) return;

        currentScore++;

        if (spawnedObjects.Contains(collectedObject))
        {
            spawnedObjects.Remove(collectedObject);
        }

        Debug.Log($"오브젝트 획득! 현재 점수: {currentScore} / {goal}");

        if (currentScore >= goal)
        {
            MissionClear();
        }
        else
        {
            SpawnSingleObject();
        }
    }

    private void MissionClear()
    {
        isMissionActive = false;
        Debug.Log("축하합니다! 미션 클리어!");

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        gameObject.SetActive(false);
    }
}
