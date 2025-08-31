using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GlobalVolumeKeeper : MonoBehaviour
{
    [Header("���� Volume Profile")]
    public VolumeProfile sharedProfile;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);  // �� �Ѿ�� �� �װ�
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
            Debug.Log($"�� {scene.name} �ε�� �� �۷ι� �������� ���� �Ϸ�");
        }
    }
}
