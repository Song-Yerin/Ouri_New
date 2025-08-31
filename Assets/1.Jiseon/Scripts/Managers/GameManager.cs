using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Title,
        Playing,
        Talking,
        Paused
    }

    public GameState CurrentState { get; private set; } = GameState.Title;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� �Ѿ�� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;

        Debug.Log("Game State changed to: " + newState);

        // ���� ��ȯ�� ���� �߰� ó�� ����
        switch (newState)
        {
            case GameState.Title:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Talking:
                Time.timeScale = 0f; // �Ͻ����� ���� ����
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
        }
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public bool IsTalking()
    {
        return CurrentState == GameState.Talking;
    }

    public bool IsPaused()
    {
        return CurrentState == GameState.Paused;
    }
}
