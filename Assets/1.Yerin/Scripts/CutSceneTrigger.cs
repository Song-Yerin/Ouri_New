using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider))]
public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    public string playerTag = "Player";

    private bool _playedThisSession = false;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (timeline)
            timeline.playOnAwake = false; // 에디터에서 자동재생 방지
    }
#endif

    void Awake()
    {
        if (timeline)
            timeline.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // ① 플레이어만 반응
        if (!other.CompareTag(playerTag)) return;

        // ② 한 세션(플레이 모드) 내에서 한 번만 재생
        if (_playedThisSession) return;

        _playedThisSession = true;

        // ③ 컷씬 재생
        if (timeline)
        {
            timeline.time = 0;
            timeline.Play();
        }

        // ④ 다시 안 나오게 스크립트 끄기 (씬 리로드 전까지만 유지)
        enabled = false;
    }
}
