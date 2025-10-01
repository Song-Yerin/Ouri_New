using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CatAttacker : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackRange = 2.0f;
    public float attackCooldown = 3.0f;
    public int attackDamage = 1;
    public string attackTriggerName = "Attack";

    [Header("사운드 설정")]
    public AudioClip[] approachSounds;
    public float maxSoundDistance = 20.0f;
    [Range(0, 1)] public float maxVolume = 0.8f;

    // 참조 변수들
    private Transform player;
    private PlayerFeedbackManager playerFeedback; // PlayerHealth 대신 통합 관리자 참조
    private AudioSource audioSource;
    private Animator anim;
    private float lastAttackTime = -10f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            // 플레이어에게서 통합 관리자 컴포넌트를 찾아옴
            playerFeedback = player.GetComponent<PlayerFeedbackManager>();
        }

        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        if (approachSounds != null && approachSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, approachSounds.Length);
            audioSource.clip = approachSounds[randomIndex];
            audioSource.loop = true;
            audioSource.spatialBlend = 1.0f;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        UpdateSoundVolume(distanceToPlayer);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger(attackTriggerName);

        // 이제 통합 관리자의 TakeDamage 함수만 호출하면 끝
        if (playerFeedback != null)
        {
            playerFeedback.TakeDamage(attackDamage);
        }
    }

    private void UpdateSoundVolume(float distance)
    {
        // (기존과 동일)
        if (approachSounds == null || approachSounds.Length == 0) return;
        if (distance > maxSoundDistance) audioSource.volume = 0;
        else audioSource.volume = (1 - (distance / maxSoundDistance)) * maxVolume;
    }
}
