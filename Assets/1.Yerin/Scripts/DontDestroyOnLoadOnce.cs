// DontDestroyOnLoadOnce.cs
using UnityEngine;

public class DontDestroyOnLoadOnce : MonoBehaviour
{
    static bool created;
    void Awake()
    {
        if (created) { Destroy(gameObject); return; }
        created = true;
        DontDestroyOnLoad(gameObject);
    }
}
