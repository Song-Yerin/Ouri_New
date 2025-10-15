using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider))]
public class CutsceneTrigger : MonoBehaviour
{
    [Header("PlayableDirector (Timeline)")]
    public PlayableDirector timeline;

    [Header("Trigger Filter")]
    public string playerTag = "Player";

    [Header("Play Once Per Session")]
    public bool playOnceThisSession = true;

    private bool _fired = false;              // 이미 발화했는지
    private Collider _col;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (timeline) timeline.playOnAwake = false;
    }
#endif

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (timeline)
        {
            timeline.playOnAwake = false;
            // 루프 방지
            timeline.extrapolationMode = DirectorWrapMode.None; // 끝나면 정지
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (playOnceThisSession && _fired) return;

        _fired = true;

        // 재진입 물리적으로 차단 (가장 확실함)
        if (_col) _col.enabled = false;

        if (timeline)
        {
            timeline.time = 0;
            timeline.stopped += OnCutsceneStopped;
            timeline.Play();
        }

        if (playOnceThisSession) enabled = false; // 선택: 스크립트 자체도 비활성화
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        if (timeline) timeline.stopped -= OnCutsceneStopped;
        // 필요 시 여기서 플레이어 입력/카메라 복구
    }

    void OnDestroy()
    {
        if (timeline) timeline.stopped -= OnCutsceneStopped;
    }
}
