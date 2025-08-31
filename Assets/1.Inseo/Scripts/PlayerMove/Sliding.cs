using UnityEngine;
namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    [RequireComponent(typeof(CharacterController))]
    public class Sliding : MonoBehaviour
    {
        [Header("슬라이드 설정")]
        [SerializeField] private KeyCode slideKey = KeyCode.F;
        [Tooltip("슬라이드 가능한 표면을 탐지할 거리입니다.")]
        [SerializeField] private float checkDistance = 0.5f;
        [SerializeField] private LayerMask slideLayerMask = -1;

        [Header("슬라이드 성능")]
        [Tooltip("슬라이드 중 마찰력 (0에 가까울수록 오래 미끄러짐)")]
        [SerializeField, Range(0.01f, 1f)] private float slideFriction = 0.1f;
        [Tooltip("경사면에서 미끄러지는 힘")]
        [SerializeField] private float slideGravityForce = 7.5f;
        [Tooltip("WASD로 방향을 제어하는 힘")]
        [SerializeField] private float slideControlForce = 10f;

        private CreatureMover creatureMover;
        private CharacterController characterController;

        private bool isSliding = false;
        private bool canSlide = false;
        private Vector3 groundNormal; // 밟고 있는 바닥의 경사

        private void Awake()
        {
            creatureMover = GetComponent<CreatureMover>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            CheckForSlideSurface();

            // 슬라이드 중에 슬라이드 불가능한 지역으로 갔다면 즉시 중지
            if (isSliding && !canSlide)
            {
                StopSliding();
            }

            // 슬라이드 가능한 지역에서 F키를 눌렀을 때 토글
            if (canSlide && Input.GetKeyDown(slideKey))
            {
                if (!isSliding)
                {
                    StartSliding();
                }
                else
                {
                    StopSliding();
                }
            }
        }

        private void CheckForSlideSurface()
        {
            // 캐릭터 발밑으로 Ray를 쏴서 현재 밟고 있는 땅의 정보를 가져옵니다.
            Vector3 rayOrigin = transform.position + characterController.center;
            float rayLength = (characterController.height / 2f) + checkDistance;

            if (Physics.SphereCast(rayOrigin, characterController.radius, Vector3.down, out RaycastHit hit, rayLength, slideLayerMask))
            {
                // 밟고 있는 땅의 태그가 "Slide"이면 슬라이드 가능
                if (hit.collider.CompareTag("Slide"))
                {
                    canSlide = true;
                    groundNormal = hit.normal; // 경사면 정보 저장
                    return;
                }
            }
            // 그 외의 모든 경우는 슬라이드 불가능
            canSlide = false;
            groundNormal = Vector3.up;
        }

        private void StartSliding()
        {
            isSliding = true;
            // CreatureMover에 새로운 슬라이드 모드 시작을 요청합니다.
            // 모든 성능 관련 파라미터를 여기서 직접 전달합니다.
            creatureMover.StartNewSlideMode(groundNormal, slideFriction, slideGravityForce, slideControlForce);
        }

        private void StopSliding()
        {
            isSliding = false;
            // CreatureMover에 슬라이드 모드 종료를 요청합니다.
            creatureMover.StopNewSlideMode();
        }
    }
}
