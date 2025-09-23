using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 연결")]
    public Slider masterVolumeSlider;       // Slider 컴포넌트 드래그
    public Toggle fullScreenToggle;         // Toggle 컴포넌트 드래그
    public TMP_Dropdown qualityDropdown;        // Dropdown 컴포넌트 드래그
    public Slider mouseSensitivitySlider;   // Slider 컴포넌트 드래그

    public SmoothMouseLook smoothMouseLook;
    public GameObject settingsPanel;
    public GameObject ppVolumeObject;
    public GameObject titleGroup;
    public Button continueButton;   // Continue 버튼 인스펙터에서 연결
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 1) 마스터 볼륨
        float vol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = vol;
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = vol;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // 2) 전체화면
        bool fs = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        Screen.fullScreen = fs;
        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = fs;
            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
        }

        // 3) 그래픽 품질 (TMP 버전)
        int ql = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(ql);

        if (qualityDropdown != null)
        {
            // 값 덮어쓰기
            qualityDropdown.value = ql;
            // 드롭다운 텍스트도 갱신
            qualityDropdown.RefreshShownValue();

            // UI 이벤트에 함수 연결
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // 4) 마우스 감도
        float sens = PlayerPrefs.GetFloat(
            "MouseSensitivity",
            smoothMouseLook != null ? smoothMouseLook.sensitivity : 1f
        );
        if (smoothMouseLook != null)
            smoothMouseLook.sensitivity = sens;
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = sens;
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        // 패널 초기화
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(false);
        if (titleGroup != null) titleGroup.SetActive(true);

        // ★ nightMapProgress 값 확인해서 Continue 버튼 활성/비활성
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);
        if (continueButton != null)
        {
            continueButton.interactable = progress != 0;
        }
    }

    private void Update()
    {
        for (int i = 0; i <= 7; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;  // KeyCode.Alpha0 ~ Alpha9 순차적으로 대응
            if (Input.GetKeyDown(key))
            {
                PlayerPrefs.SetInt("nightMapProgress", i);
                PlayerPrefs.Save();
                Debug.Log($"nightMapProgress = {i} (저장 완료)");
            }
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("FullScreen", fullscreen ? 1 : 0);
    }
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        if (smoothMouseLook != null)
            smoothMouseLook.sensitivity = sensitivity;
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }

    public void OpenSettings()
    {
        settingsPanel?.SetActive(true);
        ppVolumeObject?.SetActive(true);
        titleGroup?.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel?.SetActive(false);
        ppVolumeObject?.SetActive(false);
        titleGroup?.SetActive(true);
    }

    public void QuitGame()
    {
        foreach (var src in FindObjectsOfType<AudioSource>())
            src.Stop();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void startGame()
    {
        PlayerPrefs.SetInt("nightMapProgress", 0);
        PlayerPrefs.Save();
        Debug.Log("nightMapProgress = 0 (저장 완료)");

        LoadingSceneManager.LoadScene("FirstCutScene");
        // SceneManager.LoadScene("dialogue");
    }

    public void OnClickContinue()
    {
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);

        switch (progress)
        {
            case 0: // 기본
                LoadingSceneManager.LoadScene("FirstCutScene");
                break;
            case 1: //인공위성 내부 
                SceneManager.LoadScene("PipeScene 1");
                break;
            case 2: // 부엉이와 첫 대화
                SceneManager.LoadScene("YJ_Forest_Night");
                break;
            case 3 or 4 or 5: // 부엉이와 대화 후
                SceneManager.LoadScene("Practice");
                break;
            case 6: // 두 번째 조각 찾은 후
                SceneManager.LoadScene("YJ _Forest_Day");
                break;

            case 7:
                SceneManager.LoadScene("CityGliding");
                break;
            default:
                break;
        }
    }
}
