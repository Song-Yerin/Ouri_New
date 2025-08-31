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
    // 에디터에서 값이 바뀌어도 계속 꺼지도록 강제
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
            // ★ 체크가 안 꺼져 있어도 무력화
            timeline.playOnAwake = false;
            timeline.Stop();
            timeline.time = 0;

            // ★ 시작 시 아예 비활성화해서 자동재생 원천 차단
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
            // ★ 트리거 시점에만 활성화 후 재생
            timeline.enabled = true;
            timeline.time = 0;
            timeline.Play();
        }

        enabled = false; // 한 번만
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
