using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 연결")]
    public Slider masterVolumeSlider;       // 마스터 볼륨 슬라이더
    public Slider bgmVolumeSlider;          // BGM 전용 볼륨 슬라이더
    public Toggle fullScreenToggle;         // 전체화면 토글
    public TMP_Dropdown qualityDropdown;    // 그래픽 품질 드롭다운
    public Slider mouseSensitivitySlider;   // 마우스 감도 슬라이더

    [Header("BGM 전용 오디오소스")]
    public AudioSource bgmSource;           // 배경 BGM 오디오 소스
    public AudioSource cutsceneBgmSource;   // 컷씬용 BGM 오디오 소스

    [Header("참조")]
    public SmoothMouseLook smoothMouseLook;

    // 타이틀 전용 패널(기존)
    public GameObject settingsPanel;
    public GameObject ppVolumeObject;
    public GameObject titleGroup;
    public Button continueButton;

    // 전역(인게임 공용) 설정창
    public GameObject settingsWindow;
    public GameObject settingsButton;
    public GameObject closeButton;

    // 씬 이름 집합
    private readonly string[] titleSceneNames = { "TitleScene", "Title" };
    private readonly string[] disabledButtonScenes = { "FIrstCutScene", "LoadingScene", "Loading" };

    // 내부 상태 변수
    private float masterVolume = 1f;
    private float bgmVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 전역 SettingsCanvas 유지
        if (settingsWindow != null)
        {
            Canvas parentCanvas = settingsWindow.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
            {
                var rootGO = parentCanvas.transform.root.gameObject;
                DontDestroyOnLoad(rootGO);
            }
            settingsWindow.SetActive(false);
        }

        if (settingsButton != null) DontDestroyOnLoad(settingsButton);
        if (closeButton != null)
        {
            DontDestroyOnLoad(closeButton);
            closeButton.SetActive(false);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // 1) 마스터 볼륨
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = masterVolume;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // 2) BGM 볼륨 (마스터와 곱연산 구조)
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        ApplyBgmVolume(); // 초기값 반영

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = bgmVolume;
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        // 3) 전체화면
        bool fs = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        Screen.fullScreen = fs;
        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = fs;
            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
        }

        // 4) 그래픽 품질
        int ql = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(ql);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = ql;
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // 5) 마우스 감도
        float sens = PlayerPrefs.GetFloat("MouseSensitivity", smoothMouseLook != null ? smoothMouseLook.sensitivity : 1f);
        if (smoothMouseLook != null) smoothMouseLook.sensitivity = sens;
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = sens;
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        // 타이틀 전용 초기화
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(false);
        if (titleGroup != null) titleGroup.SetActive(true);

        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);
        if (continueButton != null) continueButton.interactable = progress != 0;
    }

    void Update()
    {
        // 개발용 단축키: 0~7 저장
        for (int i = 0; i <= 8; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (Input.GetKeyDown(key))
            {
                PlayerPrefs.SetInt("nightMapProgress", i);
                PlayerPrefs.Save();
                Debug.Log($"nightMapProgress = {i} (저장 완료)");
            }
        }

        // ESC키로 설정창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isTitle = IsIn(sceneName, titleSceneNames);
            bool isDisabled = IsIn(sceneName, disabledButtonScenes);
            if (!isTitle && !isDisabled)
            {
                ToggleSettingsWindow();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (settingsButton != null)
            settingsButton.SetActive(!(IsIn(sceneName, titleSceneNames) || IsIn(sceneName, disabledButtonScenes)));

        if (closeButton != null) closeButton.SetActive(false);
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (IsIn(sceneName, titleSceneNames) && titleGroup != null) titleGroup.SetActive(true);
    }

    public void ToggleSettingsWindow()
    {
        if (settingsWindow == null)
        {
            Debug.LogError("settingsWindow가 비어 있습니다. Inspector에서 연결하세요.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        bool isTitleScene = IsIn(sceneName, titleSceneNames);

        if (isTitleScene)
        {
            if (titleGroup != null) titleGroup.SetActive(false);
            settingsWindow.SetActive(true);
            if (closeButton != null) closeButton.SetActive(true);
            if (settingsButton != null) settingsButton.SetActive(false);
            return;
        }

        bool isActive = settingsWindow.activeSelf;
        settingsWindow.SetActive(!isActive);
        if (settingsButton != null) settingsButton.SetActive(isActive);
        if (closeButton != null) closeButton.SetActive(!isActive);
    }

    public void CloseSettingsWindow()
    {
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;
        bool showBtn = !(IsIn(sceneName, titleSceneNames) || IsIn(sceneName, disabledButtonScenes));
        if (settingsButton != null) settingsButton.SetActive(showBtn);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(true);
        if (titleGroup != null) titleGroup.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;
        bool isTitleScene = IsIn(sceneName, titleSceneNames);

        if (isTitleScene)
        {
            if (titleGroup != null) titleGroup.SetActive(true);
        }
        else
        {
            if (settingsButton != null) settingsButton.SetActive(true);
        }
    }

    // ===================== 볼륨 및 설정 저장 함수 =====================

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        AudioListener.volume = volume;

        // BGM 실제 볼륨도 반영 (마스터 × BGM)
        ApplyBgmVolume();

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;

        // 실제 적용
        ApplyBgmVolume();

        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    private void ApplyBgmVolume()
    {
        float effectiveVolume = masterVolume * bgmVolume;

        if (bgmSource != null) bgmSource.volume = effectiveVolume;
        if (cutsceneBgmSource != null) cutsceneBgmSource.volume = effectiveVolume;
    }

    public void SetFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("FullScreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        if (smoothMouseLook != null) smoothMouseLook.sensitivity = sensitivity;
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
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
    }

    public void OnClickContinue()
    {
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);

        switch (progress)
        {
            case 0:
                LoadingSceneManager.LoadScene("FirstCutScene");
                break;
            case 1:
                SceneManager.LoadScene("PipeScene 1");
                break;
            case 2:
                SceneManager.LoadScene("YJ_Forest_Night");
                break;
            case 3:
            case 4:
            case 5:
                SceneManager.LoadScene("Practice");
                break;
            case 6:
                SceneManager.LoadScene("YJ _Forest_Day");
                break;
            case 7:
                SceneManager.LoadScene("CityGliding 2");
                break;
            case 8:
                SceneManager.LoadScene("SpaceShipLauncher");
                break;
            default:
                break;
        }
    }

    private bool IsIn(string value, string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (value == list[i]) return true;
        }
        return false;
    }
}
