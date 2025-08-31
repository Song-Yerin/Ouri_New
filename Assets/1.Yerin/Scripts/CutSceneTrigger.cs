using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    public string playerTag = "Player";
    public string customSaveKey = "";

    bool _playedThisSession;

    string SaveKey =>
        string.IsNullOrEmpty(customSaveKey)
        ? $"cutscene_seen::{SceneManager.GetActiveScene().path}::{name}"
        : customSaveKey;

#if UNITY_EDITOR
    // �����Ϳ��� ���� �ٲ� ��� �������� ����
    void OnValidate()
    {
        if (timeline)
        {
            timeline.playOnAwake = false;
        }
    }
#endif

    void Awake()
    {
        if (timeline)
        {
            // �� üũ�� �� ���� �־ ����ȭ
            timeline.playOnAwake = false;
            timeline.Stop();
            timeline.time = 0;

            // �� ���� �� �ƿ� ��Ȱ��ȭ�ؼ� �ڵ���� ��õ ����
            timeline.enabled = false;
        }

        if (HasSeen()) enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (!other.CompareTag(playerTag)) return;
        if (_playedThisSession || HasSeen()) return;

        MarkSeen();

        if (timeline)
        {
            // �� Ʈ���� �������� Ȱ��ȭ �� ���
            timeline.enabled = true;
            timeline.time = 0;
            timeline.Play();
        }

        enabled = false; // �� ����
    }

    bool HasSeen() => PlayerPrefs.GetInt(SaveKey, 0) == 1;

    void MarkSeen()
    {
        _playedThisSession = true;
        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Seen Flag")]
    void ResetSeenFlag()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        _playedThisSession = false;
        enabled = true;
    }
}
