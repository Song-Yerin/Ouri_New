using UnityEngine;

/// <summary>
/// 플레이어의 동작 사운드를 관리하고, Animation Event를 통해 사운드를 재생합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("사운드 클립 목록")]
    [Tooltip("점프, 착지, 슬라이드 등 모든 효과음을 여기에 등록합니다.")]
    public AudioClip[] soundClips;

    [Header("오디오 소스")]
    [Tooltip("점프, 착지 등 단발성 효과음을 재생할 AudioSource")]
    private AudioSource oneShotAudioSource;

    [Tooltip("슬라이드, 활공 등 반복되는 사운드를 재생할 AudioSource")]
    public AudioSource loopingAudioSource;


    void Awake()
    {
        // 이 게임 오브젝트에 붙어있는 기본 AudioSource를 가져옵니다.
        oneShotAudioSource = GetComponent<AudioSource>();
    }

    // --- Animation Event에서 직접 호출할 함수들 ---

    /// <summary>
    /// Animation Event에서 호출됩니다. soundClips 배열에서 해당 인덱스의 사운드를 한 번 재생합니다.
    /// </summary>
    /// <param name="index">soundClips 배열의 인덱스 번호</param>
    public void PlaySoundByIndex(int index)
    {
        if (oneShotAudioSource == null)
        {
            return;
        }

        if (index < 0 || index >= soundClips.Length)
        {
            Debug.LogWarning("PlayerSoundController: 잘못된 사운드 인덱스입니다: " + index);
            return;
        }

        // PlayOneShot은 기존에 재생 중인 사운드를 멈추지 않고 새 사운드를 겹쳐서 재생합니다.
        oneShotAudioSource.PlayOneShot(soundClips[index]);
    }

    /// <summary>
    /// Animation Event에서 호출됩니다. 루핑 사운드를 재생 시작합니다.
    /// </summary>
    /// <param name="index">soundClips 배열의 인덱스 번호</param>
    public void StartLoopingSound(int index)
    {
        if (loopingAudioSource == null) return;
        if (index < 0 || index >= soundClips.Length) return;

        // 이미 같은 사운드가 재생 중이면 다시 시작하지 않음
        if (loopingAudioSource.isPlaying && loopingAudioSource.clip == soundClips[index]) return;

        loopingAudioSource.clip = soundClips[index];
        loopingAudioSource.loop = true;
        loopingAudioSource.Play();
    }


    // --- 다른 스크립트(예: CreatureMover)에서 호출할 함수 ---

    /// <summary>
    /// 루핑 사운드를 중지합니다. 슬라이딩이나 활공이 끝났을 때 호출합니다.
    /// </summary>
    public void StopLoopingSound()
    {
        if (loopingAudioSource != null && loopingAudioSource.isPlaying)
        {
            loopingAudioSource.Stop();
        }
    }
}
