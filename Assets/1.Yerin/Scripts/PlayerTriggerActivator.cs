using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerTriggerActivator : MonoBehaviour
{
    [Header("연결할 Activator 스크립트")]
    [SerializeField] private Activator activator;

    void Reset()
    {
        // 콜라이더를 트리거로 자동 세팅
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activator != null)
            {
                activator.Activate();
            }
            else
            {
                Debug.LogWarning("[PlayerTriggerActivator] Activator가 연결되지 않았습니다.");
            }
        }
    }
}

