using UnityEngine;

public class InventoryBootstrap : MonoBehaviour
{
    private static GameObject s_instance;
    [SerializeField] private GameObject inventoryRootPrefab; // Canvas 포함 or 별도

    void Awake()
    {
        if (s_instance != null) { Destroy(gameObject); return; }
        s_instance = Instantiate(inventoryRootPrefab);
        DontDestroyOnLoad(s_instance);    // UI 유지
        DontDestroyOnLoad(gameObject);    // 부트스트랩 유지(중복 방지)
    }
}