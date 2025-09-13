using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnKey : MonoBehaviour
{
    void OnEnable()
    {
        // 씬이 로드될 때마다 OnSceneLoaded 실행되도록 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 씬 언로드될 때 이벤트 해제 (중복 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // R 키 누르면 씬 리로드
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed! Restarting scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 씬 리로드 직후 호출되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene reloaded: " + scene.name);

        // 마우스 커서 복원 (UI 클릭 가능하도록)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }
}
