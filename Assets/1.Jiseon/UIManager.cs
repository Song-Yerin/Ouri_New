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
    public TMP_Dropdown qualityDropdown;    // Dropdown 컴포넌트 드래그
    public Slider mouseSensitivitySlider;   // Slider 컴포넌트 드래그

    [Header("참조")]
    public SmoothMouseLook smoothMouseLook;

    // 타이틀 전용 패널(기존)
    public GameObject settingsPanel;        // 타이틀 전용 설정 패널
    public GameObject ppVolumeObject;       // 타이틀 전용 후처리 오브젝트
    public GameObject titleGroup;           // 타이틀 UI 그룹
    public Button continueButton;           // Continue 버튼

    // 전역(인게임 공용) 설정창
    public GameObject settingsWindow;       // SettingsCanvas 안의 SettingsWindow(패널)
    public GameObject settingsButton;       // 인게임용 Settings 버튼(전역)
    public GameObject closeButton;          // 전역 SettingsWindow 안 닫기 버튼

    // 씬 이름 집합
    private readonly string[] titleSceneNames = { "TitleScene", "Title" };
    private readonly string[] disabledButtonScenes = { "FIrstCutScene", "LoadingScene", "Loading" };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 전역 SettingsCanvas 유지: settingsWindow의 부모 Canvas를 찾아 루트에 DDOL 적용
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

        // 전역 인게임용 버튼과 닫기 버튼 유지
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
            qualityDropdown.value = ql;
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // 4) 마우스 감도
        float sens = PlayerPrefs.GetFloat("MouseSensitivity", smoothMouseLook != null ? smoothMouseLook.sensitivity : 1f);
        if (smoothMouseLook != null) smoothMouseLook.sensitivity = sens;
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = sens;
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        // 타이틀 전용 패널 초기화
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(false);
        if (titleGroup != null) titleGroup.SetActive(true);

        // Continue 버튼 활성화
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);
        if (continueButton != null) continueButton.interactable = progress != 0;
    }

    void Update()
    {
        // 개발 편의: 숫자키 0~7로 진행도 저장 (원본 유지)
        for (int i = 0; i <= 7; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (Input.GetKeyDown(key))
            {
                PlayerPrefs.SetInt("nightMapProgress", i);
                PlayerPrefs.Save();
                Debug.Log($"nightMapProgress = {i} (저장 완료)");
            }
        }

        // ESC 키로 전역 SettingsWindow 토글 (타이틀/비활성 씬 제외)
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

        // 인게임용 settingsButton은 타이틀, 컷씬, 로딩씬에서 숨김
        if (settingsButton != null)
            settingsButton.SetActive(!(IsIn(sceneName, titleSceneNames) || IsIn(sceneName, disabledButtonScenes)));

        // 전역 닫기 버튼 기본 숨김
        if (closeButton != null) closeButton.SetActive(false);

        // 전역 SettingsWindow는 씬 진입 시 항상 닫음
        if (settingsWindow != null) settingsWindow.SetActive(false);

        // 타이틀 씬에서는 타이틀 UI 노출
        if (IsIn(sceneName, titleSceneNames) && titleGroup != null) titleGroup.SetActive(true);
    }

    // 전역 SettingsWindow 토글 (인게임에서 사용)
    public void ToggleSettingsWindow()
    {
        if (settingsWindow == null)
        {
            Debug.LogError("settingsWindow가 비어 있습니다. Inspector에서 연결하세요.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        bool isTitleScene = false;
        foreach (string name in titleSceneNames)
        {
            if (sceneName == name)
            {
                isTitleScene = true;
                break;
            }
        }

        // -------------------------------
        // ① 타이틀씬 전용 동작
        // -------------------------------
        if (isTitleScene)
        {
            // 타이틀 UI 끄고 세팅창 켜기 (기존 OpenSettings와 동일)
            if (titleGroup != null)
                titleGroup.SetActive(false);

            settingsWindow.SetActive(true);
            if (closeButton != null)
                closeButton.SetActive(true);

            // 타이틀씬에는 인게임용 세팅버튼 없음
            if (settingsButton != null)
                settingsButton.SetActive(false);

            Debug.Log("타이틀씬 세팅창 열림");
            return;
        }

        // -------------------------------
        // ② 인게임용 동작
        // -------------------------------
        bool isActive = settingsWindow.activeSelf;

        // 세팅창 열기
        if (!isActive)
        {
            settingsWindow.SetActive(true);

            if (settingsButton != null)
                settingsButton.SetActive(false);

            if (closeButton != null)
                closeButton.SetActive(true);
        }
        // 세팅창 닫기
        else
        {
            settingsWindow.SetActive(false);

            if (settingsButton != null)
                settingsButton.SetActive(true);

            if (closeButton != null)
                closeButton.SetActive(false);
        }
    }


    // 전역 SettingsWindow 닫기 (닫기 버튼에 연결)
    public void CloseSettingsWindow()
    {
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;
        bool showBtn = !(IsIn(sceneName, titleSceneNames) || IsIn(sceneName, disabledButtonScenes));
        if (settingsButton != null) settingsButton.SetActive(showBtn);
    }

    // 타이틀 전용: 기존 설정창 열기
    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(true);
        if (titleGroup != null) titleGroup.SetActive(false);
    }

    // 타이틀 전용: 기존 설정창 닫기
    public void CloseSettings()
    {
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        if (closeButton != null)
            closeButton.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;
        bool isTitleScene = false;
        foreach (string name in titleSceneNames)
        {
            if (sceneName == name)
            {
                isTitleScene = true;
                break;
            }
        }

        if (isTitleScene)
        {
            // 타이틀씬에서는 닫으면 titleGroup 다시 켜기
            if (titleGroup != null)
                titleGroup.SetActive(true);
        }
        else
        {
            // 인게임에서는 세팅 버튼 복구
            if (settingsButton != null)
                settingsButton.SetActive(true);
        }
    }


    // 설정 저장 함수들 (원본 유지 + Save 추가)
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
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
        // SceneManager.LoadScene("dialogue");
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
            default:
                break;
        }
    }

    // 유틸: 문자열이 배열에 포함돼 있는지 검사
    private bool IsIn(string value, string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (value == list[i]) return true;
        }
        return false;
    }
}
