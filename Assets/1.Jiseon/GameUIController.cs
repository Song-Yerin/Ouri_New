using UnityEngine;
using TMPro; // TextMeshPro 사용 시
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI lifeText;     // 생명 UI
    public TextMeshProUGUI clockText;    // 시계 UI

    [Header("Script References")]
    public PlayerFeedbackManager playerFeedback;
    public MissionManager missionManager;

    [Tooltip("UI 갱신 간격 (성능 최적화용)")]
    public float updateInterval = 0.1f;
    private float timer;

    void Start()
    {
        if (playerFeedback == null)
            playerFeedback = FindObjectOfType<PlayerFeedbackManager>();

        if (missionManager == null)
            missionManager = FindObjectOfType<MissionManager>();

        UpdateUI(); // 시작 시 즉시 한 번 갱신
    }

    void Update()
    {
        UpdateUI();

    }

    private void UpdateUI()
    {
        if (playerFeedback != null && lifeText != null)
        {
            // PlayerFeedbackManager 내부 currentHealth는 private이라면 public getter 만들어줘야 함
            int current = GetCurrentHealth();
            int max = playerFeedback.maxHealth;
            lifeText.text = $"x{current}";
        }

        if (missionManager != null && clockText != null)
        {
            int current = GetCurrentScore();
            int goal = missionManager.goal;
            clockText.text = $"{current}/{goal}";
        }
    }

    // PlayerFeedbackManager의 private 변수 접근용 (getter 필요할 때)
    private int GetCurrentHealth()
    {
        // PlayerFeedbackManager에서 currentHealth가 private이라면 public 프로퍼티로 노출 필요:
        // public int CurrentHealth => currentHealth;
        return playerFeedback.GetType().GetField("currentHealth",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null
               ? (int)playerFeedback.GetType().GetField("currentHealth",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .GetValue(playerFeedback)
               : playerFeedback.maxHealth;
    }

    private int GetCurrentScore()
    {
        // MissionManager에서 currentScore가 private이라면 public 프로퍼티 추가 필요:
        // public int CurrentScore => currentScore;
        return missionManager.GetType().GetField("currentScore",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null
               ? (int)missionManager.GetType().GetField("currentScore",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .GetValue(missionManager)
               : 0;
    }
}
