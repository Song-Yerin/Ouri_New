using UnityEngine;

namespace Controller
{
    /// <summary>
    /// 플레이어의 손 위치를 기준으로 벽 매달리기를 감지하는 컴포넌트
    /// </summary>
    public class WallHangDetector : MonoBehaviour
    {
        [Header("Hand Positions")]
        [Tooltip("왼손 Transform (애니메이터의 왼손 본 또는 수동 지정)")]
        [SerializeField] private Transform leftHandTransform;

        [Tooltip("오른손 Transform (애니메이터의 오른손 본 또는 수동 지정)")]
        [SerializeField] private Transform rightHandTransform;

        [Tooltip("손 Transform이 없을 경우 사용할 오프셋 (플레이어 중심 기준)")]
        [SerializeField] private Vector3 handOffset = new Vector3(0, 1.5f, 0.5f);

        [Header("Detection Settings")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float detectionRadius = 0.3f;
        [SerializeField] private float hangDistance = 0.6f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 8f;

        [Header("Cooldown")]
        [Tooltip("매달린 후 다시 매달리기까지 필요한 쿨다운 (지면 착지 전까지)")]
        [SerializeField] private bool requireGroundedToReset = true;

        [Header("Visual")]
        [SerializeField] private bool showVisualSphere = true;
        [SerializeField] private Color detectionColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private GameObject visualSphere;

        [Header("Debug")]
        [SerializeField] private bool showDebug = true;

        private CreatureMover creatureMover;
        private CharacterController controller;
        private bool isHanging = false;
        private bool hasHungThisJump = false; // [추가] 이번 점프에서 이미 매달렸는지
        private Vector3 hangWallNormal;
        private Vector3 hangPosition;

        public bool IsHanging => isHanging;

        private void Awake()
        {
            creatureMover = GetComponent<CreatureMover>();
            controller = GetComponent<CharacterController>();

            // 손 Transform 자동 찾기
            if (leftHandTransform == null || rightHandTransform == null)
            {
                Animator animator = GetComponent<Animator>();
                if (animator != null)
                {
                    leftHandTransform = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    rightHandTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
                }
            }

            // 시각적 Sphere 생성
            if (showVisualSphere && visualSphere == null)
            {
                CreateVisualSphere();
            }
        }

        private void CreateVisualSphere()
        {
            visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualSphere.name = "WallHangDetector_Visual";
            visualSphere.transform.SetParent(transform);

            // Collider 제거 (시각용이므로)
            Destroy(visualSphere.GetComponent<Collider>());

            // 반투명 Material 생성
            Renderer renderer = visualSphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = detectionColor;
            renderer.material = mat;

            visualSphere.transform.localScale = Vector3.one * detectionRadius * 2f;
        }

        private void Update()
        {
            // [추가] 지면 착지 시 쿨다운 리셋
            if (requireGroundedToReset && controller.isGrounded && !isHanging)
            {
                if (hasHungThisJump)
                {
                    hasHungThisJump = false;
                    Debug.Log("[WallHang] 지면 착지 - 매달리기 쿨다운 리셋");
                }
            }

            if (isHanging)
            {
                HandleHangingInput();
            }
            else
            {
                CheckWallHang();
            }

            // Visual Sphere 위치 업데이트
            UpdateVisualSphere();
        }

        private void UpdateVisualSphere()
        {
            if (visualSphere != null && showVisualSphere)
            {
                Vector3 handPosition = GetHandPosition();
                visualSphere.transform.position = handPosition;
                visualSphere.SetActive(!isHanging && !controller.isGrounded); // 공중일 때만 표시
            }
            else if (visualSphere != null && !showVisualSphere)
            {
                visualSphere.SetActive(false);
            }
        }

        private void CheckWallHang()
        {
            if (controller.isGrounded)
                return;

            // 이미 이번 점프에서 매달렸으면 체크 안 함
            if (hasHungThisJump)
                return;

            Vector3 handPosition = GetHandPosition();
            Vector3 checkDirection = transform.forward;

            if (Physics.SphereCast(handPosition, detectionRadius, checkDirection, out RaycastHit hit, hangDistance, wallLayer))
            {
                // [추가] Terrain 체크 - Terrain이면 무시
                if (hit.collider.GetComponent<Terrain>() != null)
                {
                    if (showDebug)
                    {
                        Debug.DrawLine(handPosition, hit.point, Color.blue); // Terrain은 파란색으로 표시
                        Debug.Log("[WallHang] Terrain 감지 - 매달리기 불가");
                    }
                    return;
                }

                float wallAngle = Vector3.Angle(hit.normal, Vector3.up);

                if (wallAngle > 70f && wallAngle < 110f)
                {
                    TryStartHang(hit);
                }
            }

            if (showDebug)
            {
                Debug.DrawLine(handPosition, handPosition + Vector3.up * 0.2f, Color.green);
                Debug.DrawRay(handPosition, checkDirection * hangDistance, Color.yellow);
            }
        }
        private Vector3 GetHandPosition()
        {
            if (leftHandTransform != null && rightHandTransform != null)
            {
                return (leftHandTransform.position + rightHandTransform.position) / 2f;
            }
            else
            {
                return transform.position + transform.rotation * handOffset;
            }
        }

        private void TryStartHang(RaycastHit hit)
        {
            isHanging = true;
            hasHungThisJump = true; // [추가] 이번 점프에서 매달렸음 표시
            hangWallNormal = hit.normal;
            hangPosition = hit.point;

            if (creatureMover != null)
            {
                creatureMover.SetHangMode(true, hangWallNormal);
                creatureMover.ResetKinetics(true, false, true);
            }

            Debug.Log($"[WallHang] 매달리기 시작! (쿨다운 활성화)");
        }

        private void HandleHangingInput()
        {
            // 점프 키로 벽에서 점프
            if (Input.GetButtonDown("Jump"))
            {
                JumpOffWall();
                return;
            }

            // 아래 방향키로 매달림 해제
            float verticalInput = Input.GetAxis("Vertical");
            if (verticalInput < -0.5f)
            {
                ReleaseHang();
                return;
            }

            // 벽에 살짝 밀착
            Vector3 stickToWallVelocity = -hangWallNormal * 0.1f;
            controller.Move(stickToWallVelocity * Time.deltaTime);

            if (showDebug)
            {
                Debug.DrawLine(hangPosition, hangPosition + hangWallNormal * 0.5f, Color.red);
                Debug.DrawLine(transform.position, hangPosition, Color.cyan);
            }
        }

        private void JumpOffWall()
        {
            if (creatureMover == null)
            {
                ReleaseHang();
                return;
            }

            ReleaseHang();

            Vector3 jumpDirection = (hangWallNormal + Vector3.up * 1.5f).normalized;
            Vector3 jumpVelocity = jumpDirection * jumpForce;

            creatureMover.Bounce(jumpVelocity);

            Debug.Log($"[WallHang] 벽 점프!");
        }

        private void ReleaseHang()
        {
            isHanging = false;

            if (creatureMover != null)
            {
                creatureMover.SetHangMode(false, Vector3.zero);
            }

            Debug.Log("[WallHang] 매달림 해제");
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                Vector3 previewHandPos = transform.position + transform.rotation * handOffset;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(previewHandPos, detectionRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(previewHandPos, previewHandPos + transform.forward * hangDistance);
            }
            else if (isHanging)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hangPosition, 0.2f);
                Gizmos.DrawLine(transform.position, hangPosition);
            }
        }

        private void OnDestroy()
        {
            // Visual Sphere 정리
            if (visualSphere != null)
            {
                Destroy(visualSphere);
            }
        }
    }
}
