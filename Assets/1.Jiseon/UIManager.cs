using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI ����")]
    public Slider masterVolumeSlider;       // Slider ������Ʈ �巡��
    public Toggle fullScreenToggle;         // Toggle ������Ʈ �巡��
    public TMP_Dropdown qualityDropdown;        // Dropdown ������Ʈ �巡��
    public Slider mouseSensitivitySlider;   // Slider ������Ʈ �巡��

    public SmoothMouseLook smoothMouseLook;
    public GameObject settingsPanel;
    public GameObject ppVolumeObject;
    public GameObject titleGroup;
    public Button continueButton;   // Continue ��ư �ν����Ϳ��� ����
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
        // 1) ������ ����
        float vol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = vol;
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = vol;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // 2) ��üȭ��
        bool fs = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        Screen.fullScreen = fs;
        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = fs;
            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
        }

        // 3) �׷��� ǰ�� (TMP ����)
        int ql = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(ql);

        if (qualityDropdown != null)
        {
            // �� �����
            qualityDropdown.value = ql;
            // ��Ӵٿ� �ؽ�Ʈ�� ����
            qualityDropdown.RefreshShownValue();

            // UI �̺�Ʈ�� �Լ� ����
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        // 4) ���콺 ����
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

        // �г� �ʱ�ȭ
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (ppVolumeObject != null) ppVolumeObject.SetActive(false);
        if (titleGroup != null) titleGroup.SetActive(true);

        // �� nightMapProgress �� Ȯ���ؼ� Continue ��ư Ȱ��/��Ȱ��
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);
        if (continueButton != null)
        {
            continueButton.interactable = progress != 0;
        }
    }

    private void Update()
    {
        // �� �׽�Ʈ��: ���� Ű 0,1,2 ������ nightMapProgress ����
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PlayerPrefs.SetInt("nightMapProgress", 0);
            PlayerPrefs.Save();
            Debug.Log("nightMapProgress = 0 (���� �Ϸ�)");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerPrefs.SetInt("nightMapProgress", 1);
            PlayerPrefs.Save();
            Debug.Log("nightMapProgress = 1 (���� �Ϸ�)");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerPrefs.SetInt("nightMapProgress", 2);
            PlayerPrefs.Save();
            Debug.Log("nightMapProgress = 2 (���� �Ϸ�)");
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
        Debug.Log("nightMapProgress = 0 (���� �Ϸ�)");

        LoadingSceneManager.LoadScene("YR3");
        // SceneManager.LoadScene("dialogue");
    }

    public void OnClickContinue()
    {
        int progress = PlayerPrefs.GetInt("nightMapProgress", 0);

        switch (progress)
        {
            case 0: // �⺻
                LoadingSceneManager.LoadScene("YR3");
                break;
            case 1: //�ΰ����� ���� 
                SceneManager.LoadScene("YR2");
                break;
            case 2: // �ξ��̿� ��ȭ ����~~
                SceneManager.LoadScene("YJ_Forest_Night");
                break;
            default:
                break;
        }
    }
}
