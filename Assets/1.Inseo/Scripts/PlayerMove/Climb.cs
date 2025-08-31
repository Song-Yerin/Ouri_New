using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    [RequireComponent(typeof(CharacterController))]
    public class Climb : MonoBehaviour
    {
        [Header("��� ����")]
        [SerializeField] private KeyCode climbKey = KeyCode.F;
        [SerializeField] private float wallCheckDistance = 1.5f;
        [SerializeField] private LayerMask wallLayerMask = -1;

        [Tooltip("����� �����ϴ� ���� Ray�� �� Y�� ���� �������Դϴ�. �������� �� ������ �پ��ֽ��ϴ�.")]
        [SerializeField, Range(0f, 2f)] private float climbingRayOffsetY = 0.4f;

        [Header("�ð��� ȿ��")]
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
                Debug.LogWarning("[Climb] rootBone�� �Ҵ���� �ʾҽ��ϴ�.");
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
            // ���� �� �Լ��� ���� ��� ���� ���� ȸ���� ����մϴ�.
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

        // --- [�ٽ� ���� 1] ---
        private void StopClimbing()
        {
            isClimbing = false;
            creatureMover.SetClimbMode(false, Vector3.zero);

            // ��� ���� ��� ���� ȸ���� �⺻������ '���' �����մϴ�.
            if (rootBone != null)
            {
                rootBone.localRotation = Quaternion.identity;
            }
        }

        // --- [�ٽ� ���� 2] ---
        private void ApplyClimbRotation()
        {
            if (rootBone == null || !isClimbing)
            {
                // ��� ���� �ƴϸ� �ƹ��͵� ���� �ʰ� ��� �����մϴ�.
                return;
            }

            // ���� ��� ���� ���� ȸ�� ������ �����մϴ�.
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
