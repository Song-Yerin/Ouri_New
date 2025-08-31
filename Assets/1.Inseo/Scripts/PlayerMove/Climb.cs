using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    [RequireComponent(typeof(CharacterController))]
    public class Climb : MonoBehaviour
    {
        [Header("등반 설정")]
        [SerializeField] private KeyCode climbKey = KeyCode.F;
        [SerializeField] private float wallCheckDistance = 1.5f;
        [SerializeField] private LayerMask wallLayerMask = -1;

        [Tooltip("등반을 유지하는 동안 Ray를 쏠 Y축 높이 오프셋입니다. 낮을수록 벽 끝까지 붙어있습니다.")]
        [SerializeField, Range(0f, 2f)] private float climbingRayOffsetY = 0.4f;

        [Header("시각적 효과")]
        [SerializeField] private Transform rootBone;
        [SerializeField] private float rotationSpeed = 10f;

        private CreatureMover creatureMover;
        private CharacterController characterController;

        private bool isClimbing = false;
        private bool canClimb = false;
        private Vector3 wallNormal;

        private void Awake()
        {
            creatureMover = GetComponent<CreatureMover>();
            characterController = GetComponent<CharacterController>();

            if (rootBone == null)
            {
                Debug.LogWarning("[Climb] rootBone이 할당되지 않았습니다.");
            }
        }

        private void Update()
        {
            CheckForWall();

            if (isClimbing && !canClimb)
            {
                StopClimbing();
            }

            if (Input.GetKeyDown(climbKey))
            {
                if (!isClimbing && canClimb)
                {
                    StartClimbing();
                }
                else if (isClimbing)
                {
                    StopClimbing();
                }
            }
        }

        private void LateUpdate()
        {
            // 이제 이 함수는 오직 등반 중일 때만 회전을 담당합니다.
            ApplyClimbRotation();
        }

        private void CheckForWall()
        {
            float rayOffsetY = isClimbing ? climbingRayOffsetY : (characterController.height * 0.5f);
            var rayOrigin = transform.position + Vector3.up * rayOffsetY;

            canClimb = false;

            if (isClimbing)
            {
                var directionToWall = -wallNormal;
                Debug.DrawRay(rayOrigin, directionToWall * wallCheckDistance, Color.blue);

                if (Physics.Raycast(rayOrigin, directionToWall, out RaycastHit hit, wallCheckDistance, wallLayerMask) && hit.collider.CompareTag("Climb"))
                {
                    canClimb = true;
                    wallNormal = hit.normal;
                }
            }
            else
            {
                var rayDirection = transform.forward;
                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, wallCheckDistance, wallLayerMask) && hit.collider.CompareTag("Climb"))
                {
                    canClimb = true;
                    wallNormal = hit.normal;
                }
            }
        }

        private void StartClimbing()
        {
            isClimbing = true;
            creatureMover.SetClimbMode(true, wallNormal);
        }

        // --- [핵심 수정 1] ---
        private void StopClimbing()
        {
            isClimbing = false;
            creatureMover.SetClimbMode(false, Vector3.zero);

            // 등반 종료 즉시 모델의 회전을 기본값으로 '즉시' 복구합니다.
            if (rootBone != null)
            {
                rootBone.localRotation = Quaternion.identity;
            }
        }

        // --- [핵심 수정 2] ---
        private void ApplyClimbRotation()
        {
            if (rootBone == null || !isClimbing)
            {
                // 등반 중이 아니면 아무것도 하지 않고 즉시 종료합니다.
                return;
            }

            // 오직 등반 중일 때만 회전 로직을 실행합니다.
            Vector3 newForward = Vector3.up;
            Vector3 newUp = wallNormal;
            Quaternion targetRotation = Quaternion.LookRotation(newForward, newUp);

            rootBone.localRotation = Quaternion.Slerp(
                rootBone.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}
