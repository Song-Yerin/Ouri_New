using UnityEngine;
using Controller; // CreatureMover 네임스페이스
using System;
using System.Collections.Generic;

public class GimmickSegment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CreatureMover mover;
    [SerializeField] private CharacterController controller;

    [Header("Segment Markers")]
    [SerializeField] private Collider startZone;   // 트리거 릴레이에서 OnStartEntered 호출
    [SerializeField] private Collider endZone;     // 트리거 릴레이에서 OnEndEntered 호출
    [SerializeField] private Transform spawnPoint; // 시작 지점

    [Header("Options")]
    [SerializeField] private float graceAfterRespawn = 0.3f;

    // ===== Rings =====
    [Serializable]
    public struct RingNode
    {
        public GlideRingAccelerator accelerator;
        public AudioClip passSfx;
    }
    [Header("Rings")]
    [SerializeField] private RingNode[] rings;

    // 순서 무시 진행 집계
    private bool[] ringCleared;   // 각 링을 '한 번이라도' 통과했는지
    private bool[] missCharged;   // 앞쪽 미클리어 링 중 이미 패널티 청구된 링 표시
    private int clearedUnique;    // 고유 통과 개수
    private int currentRing;      // 다음에 깨야 할 "가장 앞 미통과" 인덱스

    public int TotalRings => rings != null ? rings.Length : 0;
    public int ClearedCount => Mathf.Clamp(clearedUnique, 0, TotalRings);

    // ===== Audio =====
    [Header("Audio")]
    [SerializeField] private AudioClip segmentLoop;
    [SerializeField] private AudioClip enterSfx, exitSfx;
    [SerializeField] private AudioSource segmentSource;
    [SerializeField, Range(0f, 1f)] private float loopVolume = 0.8f;
    [SerializeField] private float fadeSeconds = 0.25f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;
    [SerializeField] private bool followPlayerFor3D = true;

    // ===== Lives & UI =====
    [Header("Lives/UI")]
    [SerializeField] private int extraLivesMax = 3;
    private int extraLives;

    public int LivesLeft => Mathf.Max(0, extraLives);
    public int TotalLives => Mathf.Max(0, extraLivesMax);

    public event Action<int, int> OnRingProgressChanged; // (cleared, total)
    public event Action<int, int> OnLivesChanged;        // (left, total)
    [SerializeField] private SegmentUIBinder uiBinder;

    // ===== Runtime State =====
    [SerializeField] private bool debugLogs = false;
    private bool active;     // 스타트존 진입 후에만 true
    private bool completed;
    private bool suppressGroundFailUntilStart = true; // 스타트 전엔 항상 true
    private float graceTimer;
    private Coroutine fadeRoutine;

    // 이벤트 구독 관리
    private readonly Dictionary<GlideRingAccelerator, Action<GlideRingAccelerator, Collider>> _subs
        = new Dictionary<GlideRingAccelerator, Action<GlideRingAccelerator, Collider>>();

    // ─────────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (!segmentSource)
        {
            segmentSource = gameObject.AddComponent<AudioSource>();
            segmentSource.playOnAwake = false;
            segmentSource.loop = true;
        }
        segmentSource.spatialBlend = spatialBlend;
        segmentSource.volume = 0f;

        // 스타트 전 상태 보장
        active = false;
        suppressGroundFailUntilStart = true;

        EnsureRingArrays();
    }

    private void OnEnable()
    {
        // 링 이벤트 구독
        _subs.Clear();
        EnsureRingArrays();

        for (int i = 0; i < TotalRings; i++)
        {
            int idx = i;
            var acc = rings[i].accelerator;
            if (!acc) continue;

            Action<GlideRingAccelerator, Collider> cb = (who, other) =>
            {
                if (!active || completed) return;
                if (!other || !player) return;
                if (other.transform.root != player.root) return;

                // 이미 깬 링이면 무시
                if (ringCleared[idx]) return;

                int penalty = 0;
                for (int j = 0; j < idx; j++)
                {
                    if (!ringCleared[j] && !missCharged[j])
                    {
                        missCharged[j] = true; // 이제 이 링은 청구 완료 표시
                        penalty++;
                    }
                }
                // 패널티 일괄 적용
                if (penalty > 0)
                {
                    extraLives = Mathf.Max(0, extraLives - penalty);
                    OnLivesChanged?.Invoke(LivesLeft, TotalLives);

                    // 목숨 0 → 즉시 게임오버(이번 링은 클리어로 인정하지 않음)
                    if (extraLives == 0)
                    {
                        ForceGameOverRespawn();
                        return;
                    }
                }

                // 여기 오면 클리어 인정
                ringCleared[idx] = true;
                clearedUnique++;
                OnRingProgressChanged?.Invoke(ClearedCount, TotalRings);

                // 다음 기대 링 재계산(가장 앞 미통과)
                RecomputeCurrentRing();

                // SFX
                var pass = rings[idx].passSfx;
                if (pass) AudioSource.PlayClipAtPoint(pass, player ? player.position : transform.position);
            };

            
            acc.OnRingPassed += cb;
            _subs[acc] = cb;
        }
    }

    private void OnDisable()
    {
        foreach (var kv in _subs)
            if (kv.Key) kv.Key.OnRingPassed -= kv.Value;
        _subs.Clear();
    }

    private void Update()
    {
        if (spatialBlend > 0f && followPlayerFor3D && player)
            transform.position = player.position;

        if (!active || completed) return;

        if (graceTimer > 0f) graceTimer -= Time.deltaTime;

        bool grounded = mover != null && mover.IsActuallyGrounded;

        // 바닥 접지 = 무조건 게임오버 처리 (스타트존 다시 밟기 전까지 비활성)
        if (!suppressGroundFailUntilStart && graceTimer <= 0f && grounded)
        {
            ForceGameOverRespawn();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Public Relays
    public void OnStartEntered()
    {
        active = true;
        completed = false;

        // 진행 유지 + 기대 링만 갱신
        RecomputeCurrentRing();

        suppressGroundFailUntilStart = false;
        extraLives = extraLivesMax;

        graceTimer = 0.2f;
        PlayEnterSfx();
        StartSegmentLoop();

        uiBinder?.Show(true);
        OnRingProgressChanged?.Invoke(ClearedCount, TotalRings);
        OnLivesChanged?.Invoke(LivesLeft, TotalLives);
    }

    public void OnEndEntered()
    {
        if (!active) return;

        bool allCleared = (ClearedCount >= TotalRings); // 순서 무시 집계
        if (allCleared)
        {
            completed = true;
            active = false;
            PlayExitSfx();
            StopSegmentLoop(true);
            uiBinder?.Show(false);
        }
        else
        {
            // End 실패 = 라이프 1 감소, 0이면 게임오버 워프
            extraLives = Mathf.Max(0, extraLives - 1);
            OnLivesChanged?.Invoke(LivesLeft, TotalLives);
            if (extraLives == 0) ForceGameOverRespawn();
            // 0 아니면 진행 계속 (워프 없음)
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Teleport & States
    private void RespawnTo(Transform point, float extraGrace = 0.2f)
    {
        if (!point || !player)
        {
            if (debugLogs) Debug.LogError("[GimmickSegment] RespawnTo 실패: point/player 누락");
            return;
        }
        StartCoroutine(CoRespawnTo(point, extraGrace));
    }

    private System.Collections.IEnumerator CoRespawnTo(Transform point, float extraGrace)
    {
        if (debugLogs) Debug.Log($"[GimmickSegment] ▶ 텔레포트 준비: {point.name}");

        if (controller) controller.enabled = false;

        player.SetPositionAndRotation(point.position, point.rotation);
        Physics.SyncTransforms();

        yield return null; // 다음 프레임
        if (controller) controller.enabled = true;

        graceTimer = Mathf.Max(extraGrace, graceAfterRespawn);
        suppressGroundFailUntilStart = true;
        StartCoroutine(CoUnsuppressAfter(0.25f));

        if (debugLogs) Debug.Log($"[GimmickSegment] 텔레포트 완료 → {player.position}");
    }

    private System.Collections.IEnumerator CoUnsuppressAfter(float sec)
    {
        if (sec > 0f) yield return new WaitForSeconds(sec);
        // 스타트존 들어오기 전(active=false)이면 억제 해제 금지
        if (!active) yield break;

        suppressGroundFailUntilStart = false;
        if (debugLogs) Debug.Log("[GimmickSegment] 바닥 실패 억제 해제");
    }

    private void RespawnToStart(bool hardResetProgress)
    {
        RespawnTo(spawnPoint, 0.2f);

        active = false;
        completed = false;
        suppressGroundFailUntilStart = true;
        uiBinder?.Show(false);

        if (hardResetProgress)
        {
            // 진행도/라이프 완전 리셋
            EnsureRingArrays();
            System.Array.Fill(ringCleared, false);
            Array.Fill(missCharged, false);
            clearedUnique = 0;
            currentRing = 0;

            extraLives = extraLivesMax;
            OnLivesChanged?.Invoke(LivesLeft, TotalLives);
            OnRingProgressChanged?.Invoke(ClearedCount, TotalRings);
        }
        // BGM 재시작/억제 해제는 StartZone에서만
    }

    // 바닥 등 게임오버 공통 처리
    private void ForceGameOverRespawn()
    {
        StopSegmentLoop(true);
        active = false;
        completed = false;
        uiBinder?.Show(false);

        suppressGroundFailUntilStart = true;

        // 0을 찍고 UI 갱신
        extraLives = 0;
        OnLivesChanged?.Invoke(LivesLeft, TotalLives);

        EnsureRingArrays();                  // 안전하게 크기 보정
        System.Array.Fill(ringCleared, false);
        Array.Fill(missCharged, false);
        clearedUnique = 0;
        currentRing = 0;
        OnRingProgressChanged?.Invoke(ClearedCount, TotalRings);  // => 0/총

        // 스폰 워프
        RespawnTo(spawnPoint, 0.25f);

        // 다음 라운드 대비 리필(비활성 상태라 StartZone 전까지 사용 안 됨)
        extraLives = extraLivesMax;
        OnLivesChanged?.Invoke(LivesLeft, TotalLives);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Progress helpers
    private void EnsureRingArrays()
    {
        int n = TotalRings;
        if (n <= 0)
        {
            ringCleared = Array.Empty<bool>();
            missCharged = Array.Empty<bool>();
            clearedUnique = 0;
            currentRing = 0;
            return;
        }
        if (ringCleared == null || ringCleared.Length != n)
            ringCleared = new bool[n];
        if (missCharged == null || missCharged.Length != n)
            missCharged = new bool[n];

        // 이 함수는 "크기 보정"까지만. 실제 리셋은 리셋 함수에서 Array.Fill로 해.
    }

    private int NextUnclearedIndex()
    {
        for (int i = 0; i < TotalRings; i++)
            if (!ringCleared[i]) return i;
        return TotalRings; // 전부 통과
    }

    private void RecomputeCurrentRing()
    {
        currentRing = NextUnclearedIndex();
    }

    private int CountUnclearedBefore(int idx)
    {
        if (ringCleared == null) return 0;
        int c = 0;
        int n = Mathf.Min(idx, ringCleared.Length);
        for (int i = 0; i < n; i++)
            if (!ringCleared[i]) c++;
        return c;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Audio helpers
    private void StartSegmentLoop()
    {
        if (!segmentLoop) return;

        segmentSource.clip = segmentLoop;
        segmentSource.loop = true;
        if (!segmentSource.isPlaying) segmentSource.Play();

        StartFadeTo(loopVolume, fadeSeconds);
    }

    private void StopSegmentLoop(bool fade)
    {
        if (!segmentSource) return;
        if (!segmentSource.isPlaying) return;

        if (fade && fadeSeconds > 0f) StartFadeTo(0f, fadeSeconds, stopAfterFade: true);
        else { segmentSource.Stop(); segmentSource.volume = 0f; }
    }

    private void PlayEnterSfx()
    {
        if (enterSfx)
            AudioSource.PlayClipAtPoint(enterSfx, player ? player.position : transform.position);
    }

    private void PlayExitSfx()
    {
        if (exitSfx)
            AudioSource.PlayClipAtPoint(exitSfx, player ? player.position : transform.position);
    }

    private void StartFadeTo(float target, float seconds, bool stopAfterFade = false)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeVolume(segmentSource, target, seconds, stopAfterFade));
    }

    private System.Collections.IEnumerator FadeVolume(AudioSource src, float target, float seconds, bool stopAfter)
    {
        float start = src.volume, t = 0f;
        if (seconds <= 0f) src.volume = target;
        else
        {
            while (t < seconds)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, target, t / seconds);
                yield return null;
            }
            src.volume = target;
        }
        if (stopAfter && Mathf.Approximately(target, 0f)) src.Stop();
    }
}

