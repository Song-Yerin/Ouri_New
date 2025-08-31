using UnityEngine;
namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    [RequireComponent(typeof(CharacterController))]
    public class Sliding : MonoBehaviour
    {
        [Header("�����̵� ����")]
        [SerializeField] private KeyCode slideKey = KeyCode.F;
        [Tooltip("�����̵� ������ ǥ���� Ž���� �Ÿ��Դϴ�.")]
        [SerializeField] private float checkDistance = 0.5f;
        [SerializeField] private LayerMask slideLayerMask = -1;

        [Header("�����̵� ����")]
        [Tooltip("�����̵� �� ������ (0�� �������� ���� �̲�����)")]
        [SerializeField, Range(0.01f, 1f)] private float slideFriction = 0.1f;
        [Tooltip("���鿡�� �̲������� ��")]
        [SerializeField] private float slideGravityForce = 7.5f;
        [Tooltip("WASD�� ������ �����ϴ� ��")]
        [SerializeField] private float slideControlForce = 10f;

        private CreatureMover creatureMover;
        private CharacterController characterController;

        private bool isSliding = false;
        private bool canSlide = false;
        private Vector3 groundNormal; // ��� �ִ� �ٴ��� ���

        private void Awake()
        {
            creatureMover = GetComponent<CreatureMover>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            CheckForSlideSurface();

            // �����̵� �߿� �����̵� �Ұ����� �������� ���ٸ� ��� ����
            if (isSliding && !canSlide)
            {
                StopSliding();
            }

            // �����̵� ������ �������� FŰ�� ������ �� ���
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
            // ĳ���� �߹����� Ray�� ���� ���� ��� �ִ� ���� ������ �����ɴϴ�.
            Vector3 rayOrigin = transform.position + characterController.center;
            float rayLength = (characterController.height / 2f) + checkDistance;

            if (Physics.SphereCast(rayOrigin, characterController.radius, Vector3.down, out RaycastHit hit, rayLength, slideLayerMask))
            {
                // ��� �ִ� ���� �±װ� "Slide"�̸� �����̵� ����
                if (hit.collider.CompareTag("Slide"))
                {
                    canSlide = true;
                    groundNormal = hit.normal; // ���� ���� ����
                    return;
                }
            }
            // �� ���� ��� ���� �����̵� �Ұ���
            canSlide = false;
            groundNormal = Vector3.up;
        }

        private void StartSliding()
        {
            isSliding = true;
            // CreatureMover�� ���ο� �����̵� ��� ������ ��û�մϴ�.
            // ��� ���� ���� �Ķ���͸� ���⼭ ���� �����մϴ�.
            creatureMover.StartNewSlideMode(groundNormal, slideFriction, slideGravityForce, slideControlForce);
        }

        private void StopSliding()
        {
            isSliding = false;
            // CreatureMover�� �����̵� ��� ���Ḧ ��û�մϴ�.
            creatureMover.StopNewSlideMode();
        }
    }
}
