using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GlobalVolumeKeeper : MonoBehaviour
{
    [Header("°ø¿ë Volume Profile")]
    public VolumeProfile sharedProfile;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);  // ¾À ³Ñ¾î°¡µµ ¾È Á×°Ô
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var volume = FindObjectOfType<Volume>();
        if (volume != null)
        {
            volume.profile = sharedProfile;
            Debug.Log($"¾À {scene.name} ·ÎµåµÊ ¡æ ±Û·Î¹ú ÇÁ·ÎÆÄÀÏ Àû¿ë ¿Ï·á");
        }
    }
}
