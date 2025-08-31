using Controller; // CreatureMover 네임스페이스
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 링 완주(Outer 진입→Inner 스침→Outer 이탈) 시 CreatureMover에 임펄스를 전달하여 활강 보너스 속도를 누적
/// </summary>

public class GlideRingAccelerator : MonoBehaviour
{
    [Header("Boost Settings")]
    [Tooltip("링 완주 임펄스(수평). 6~12부터 시작 권장")]
    public float impulse = 10f;
    [Tooltip("연속 통과 스택당 배수(1.0=보너스 없음)")]
    public float chainMultiplierPerStack = 1.15f;
    [Tooltip("체인 유지 시간(초). 이 안에 다음 링 완주 시 스택 유지")]
    public float chainWindow = 3.0f;

    [Header("Rules")]
    [Tooltip("Inner를 스쳤을 때만 완주로 인정")]
    public bool requirePassThrough = true;

    [Header("Filter")]
    [Tooltip("플레이어 태그(비워두면 검사 생략)")]
    public string playerTag = "Player";

    // ─────────────────────────────────────────────────────────────────────────────
    // 체인(전역 공유)
    private static int s_ChainStacks = 0;
    private static float s_ChainTimer = 0f;

    // 상태
    private bool inOuter, inInner, touchedInner;

    // 한 바퀴당 1회 통지 보장용
    private bool notifiedThisOuter = false;

    // 플레이어 캐시
    private CharacterController cc;
    private CreatureMover mover;

    // 정상 통과 시
    public event Action<GlideRingAccelerator, Collider> OnRingPassed;

    private void Update()
    {
        if (s_ChainTimer > 0f)
        {
            s_ChainTimer -= Time.deltaTime;
            if (s_ChainTimer <= 0f)
            {
                s_ChainTimer = 0f;
                s_ChainStacks = 0;
                // Debug.Log("[GlideRing] 체인 타이머 만료 → 스택 리셋");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RingTriggerLeaf(Outer/Inner)에서 호출
    // ─────────────────────────────────────────────────────────────────────────────
    public void NotifyEnter(RingTriggerLeaf.Kind kind, Collider other)
    {
        if (!IsPlayer(other)) return;

        CachePlayer(other);

        if (kind == RingTriggerLeaf.Kind.Outer)
        {
            inOuter = true;
            notifiedThisOuter = false;   // 새 바퀴 시작 → 통지 가능 상태로 리셋
            // Debug.Log("[GlideRing] OUTER ENTER");
        }
        else // Inner
        {
            inInner = true;
            touchedInner = true;
            // Debug.Log("[GlideRing] INNER ENTER");
        }
    }

    public void NotifyExit(RingTriggerLeaf.Kind kind, Collider other)
    {
        if (!IsPlayer(other)) return;

        if (kind == RingTriggerLeaf.Kind.Inner)
        {
            inInner = false;
            // Debug.Log("[GlideRing] INNER EXIT");
            return;
        }

        if (kind == RingTriggerLeaf.Kind.Outer)
        {
            bool passOk = !requirePassThrough || touchedInner;
            // Debug.Log($"[GlideRing] OUTER EXIT → passOk={passOk}, touchedInner={touchedInner}");

            if (passOk && !notifiedThisOuter)
            {
                // 1) 부스트 적용
                GrantChainAndImpulse();

                // 2) 정상 통과 이벤트 통지 (한 바퀴 1회)
                OnRingPassed?.Invoke(this, other);
            }

            // 바깥 링에서 완전히 빠져나오면 상태 리셋
            inOuter = false;
            touchedInner = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    private void GrantChainAndImpulse()
    {
        s_ChainStacks++;
        s_ChainTimer = chainWindow;

        if (!cc || !mover)
        {
            Debug.LogWarning("[GlideRing] 부스트 실패: 플레이어 캐시가 없음");
            return;
        }

        // 진행 방향: 현재 컨트롤러 수평 속도 → 거의 0이면 바라보는 방향
        Vector3 dir = cc.velocity; dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = mover.transform.forward;

        float mul = Mathf.Pow(chainMultiplierPerStack, Mathf.Max(0, s_ChainStacks - 1));
        float finalImpulse = impulse * mul;

        // Debug.Log($"[GlideRing] 부스트 적용! dir={dir}, impulse={finalImpulse}, stacks={s_ChainStacks}");
        mover.AddGlideImpulse(dir, finalImpulse); // CreatureMover에 추가한 메서드
    }

    private bool IsPlayer(Collider col)
    {
        // CharacterController를 플레이어로 간주. 태그 조건이 있으면 추가로 검사
        if (!col.TryGetComponent<CharacterController>(out _)) return false;
        if (!string.IsNullOrEmpty(playerTag) && !col.CompareTag(playerTag)) return false;
        return true;
    }

    private void CachePlayer(Collider other)
    {
        if (!cc && other.TryGetComponent(out CharacterController _cc))
        {
            cc = _cc;
            mover = cc.GetComponent<CreatureMover>();
            // Debug.Log($"[GlideRing] Player 캐시 완료: {cc.name}");
        }
    }
}
