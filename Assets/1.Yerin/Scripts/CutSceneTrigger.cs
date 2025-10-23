using Cinemachine;
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

    [Header("Cinemachine Brain (Optional)")]
    [Tooltip("지정하지 않아도 타임라인 재생은 가능함. 카메라 전환만 비활성화됨.")]
    public GameObject cinemachineBrain; // Optional
    [Tooltip("씬에 존재하면 자동으로 찾아서 할당한다.")]
    public bool autoFindBrainIfNull = true;

    private bool _fired = false;      // 이미 발화했는지
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
            // 에디터/런타임 모두에서 타임라인이 자동 재생/루프되지 않게 고정
            timeline.playOnAwake = false;
            timeline.extrapolationMode = DirectorWrapMode.None; // 끝나면 정지
        }

        // 브레인 자동 탐색(선택)
        if (!cinemachineBrain && autoFindBrainIfNull)
        {
            var brain = FindObjectOfType<CinemachineBrain>(true);
            if (brain) cinemachineBrain = brain.gameObject;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (playOnceThisSession && _fired) return;

        _fired = true;

        // 재진입 물리적으로 차단
        if (_col) _col.enabled = false;

        if (timeline)
        {
            timeline.time = 0;

            // 브레인이 있으면 카메라 전환 활성화
            if (cinemachineBrain) cinemachineBrain.SetActive(true);

            timeline.stopped += OnCutsceneStopped;
            timeline.Play();
        }

        // if (playOnceThisSession) enabled = false; // 선택: 스크립트 자체도 비활성화
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        // 컷씬 종료 시 브레인 비활성화(있을 때만)
        if (cinemachineBrain) cinemachineBrain.SetActive(false);
    }

    void OnDisable()
    {
        if (timeline) timeline.stopped -= OnCutsceneStopped;
    }

    void OnDestroy()
    {
        if (timeline) timeline.stopped -= OnCutsceneStopped;
    }
}
