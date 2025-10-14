using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class FootstepManager : MonoBehaviour
{
    [Header("발자국 소리 설정")]
    [Tooltip("Wood_PhysicMat 이름의 물리 머티리얼에 해당하는 소리들")]
    public AudioClip[] woodClips;
    [Tooltip("Stone_PhysicMat 이름의 물리 머티리얼에 해당하는 소리들")]
    public AudioClip[] stoneClips;
    [Tooltip("Grass_PhysicMat 이름의 물리 머티리얼에 해당하는 소리들")]
    public AudioClip[] grassClips;
    [Tooltip("Metal_PhysicMat 이름의 물리 머티리얼에 해당하는 소리들")]
    public AudioClip[] metalClips;
    // 필요한 만큼 다른 재질의 오디오 클립 배열을 추가할 수 있습니다.

    [Header("레이캐스트 설정")]
    [Tooltip("레이캐스트를 시작할 위치 (보통 캐릭터의 발 위치)")]
    public Transform footTransform;
    [Tooltip("발 아래로 얼마나 멀리까지 바닥을 감지할지 결정합니다.")]
    public float raycastDistance = 0.5f;
    [Tooltip("바닥으로 인식할 레이어들을 선택합니다.")]
    public LayerMask groundLayer;

    private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource = GetComponent<AudioSource>();
            // 오디오 소스가 꺼져 있으면 소리가 나지 않으므로, Play On Awake는 비활성화합니다.
            audioSource.playOnAwake = false;
        }
    }

    // 이 함수를 애니메이션 이벤트에서 호출할 것입니다.
    public void PlayFootstepSound()
    {
        if(audioSource == null)
        {
            return;
        }
        // 발 아래로 레이저를 쏴서 닿는 오브젝트가 있는지 확인합니다.
        if (Physics.Raycast(footTransform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            // 닿은 오브젝트의 Collider에 Physic Material이 할당되어 있는지 확인합니다.
            if (hit.collider.sharedMaterial != null)
            {
                // 물리 머티리얼의 이름을 기반으로 적절한 오디오 클립을 가져옵니다.
                AudioClip clip = GetFootstepClip(hit.collider.sharedMaterial.name);

                if (clip != null)
                {
                    // 가져온 클립을 한 번 재생합니다. (PlayOneShot은 여러 소리가 겹쳐도 괜찮습니다)
                    audioSource.PlayOneShot(clip);
                }
            }
        }
    }

    private AudioClip GetFootstepClip(string materialName)
    {
        // 물리 머티리얼의 이름에서 "(Instance)" 접미사를 제거합니다.
        string cleanMaterialName = materialName.Replace(" (Instance)", "");

        switch (cleanMaterialName)
        {
            case "Wood_PhysicMat":
                return woodClips.Length > 0 ? woodClips[Random.Range(0, woodClips.Length)] : null;
            case "Stone_PhysicMat":
                return stoneClips.Length > 0 ? stoneClips[Random.Range(0, stoneClips.Length)] : null;
            case "Grass_PhysicMat":
                return grassClips.Length > 0 ? grassClips[Random.Range(0, grassClips.Length)] : null;
            case "Metal_PhysicMat":
                return metalClips.Length > 0 ? metalClips[Random.Range(0, metalClips.Length)] : null;
            // 다른 재질을 추가했다면 여기에 case를 추가합니다.
            default:
                Debug.LogWarning("정의되지 않은 물리 머티리얼입니다: " + cleanMaterialName);
                return null;
        }
    }
}
