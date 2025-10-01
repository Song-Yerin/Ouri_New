using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleObject : MonoBehaviour
{
    [HideInInspector]
    public MissionManager missionManager;

    private void Start()
    {
        //GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("충돌감지!");
        if (other.CompareTag("Player"))
        {
            if (missionManager != null)
            {
                // MissionManager에 '자기 자신(gameObject)'을 전달하여 수집되었음을 알림
                missionManager.OnObjectCollected(gameObject);
            }

            // 자기 자신을 파괴
            Destroy(gameObject);
        }
    }
}
