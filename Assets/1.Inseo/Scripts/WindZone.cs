using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(Collider))]
    public class WindZone : MonoBehaviour
    {
        [Header("Wind Zone Settings")]
        [Tooltip("�÷��̾ ���� �о� �ø��� ���� �����Դϴ�.")]
        public float power = 10f;

        private Collider zoneCollider;

        // ���� �ȿ� �ִ� ��� CreatureMover�� �����ϱ� ���� ����Ʈ�� ����մϴ�.
        // ���� ���� ���ÿ� ���� ��쵵 ����� �� �ֽ��ϴ�.
        private System.Collections.Generic.List<CreatureMover> playersInZone = new System.Collections.Generic.List<CreatureMover>();

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            CreatureMover mover = other.GetComponent<CreatureMover>();
            if (mover != null && !playersInZone.Contains(mover))
            {
                playersInZone.Add(mover);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            CreatureMover mover = other.GetComponent<CreatureMover>();
            if (mover != null)
            {
                playersInZone.Remove(mover);
            }
        }

        // ���� ȿ���� FixedUpdate���� ó���մϴ�.
        private void FixedUpdate()
        {
            // ����Ʈ�� �ִ� ��� �÷��̾ ��ȸ�մϴ�.
            foreach (CreatureMover player in playersInZone)
            {
                // IsGliding ������Ƽ�� CreatureMover�� �־�� �մϴ�.
                // ���� ���ٸ�, player.GetComponent<CreatureMover>().IsGliding ó�� �����ؾ� �մϴ�.
                // �� ���������� CreatureMover�� public bool IsGliding { get; } �� �ִٰ� �����մϴ�.

                // �÷��̾ Ȱ�� ������ ���� ���� �����մϴ�.
                if (player != null && player.IsGliding)
                {
                    // Time.fixedDeltaTime�� ���Ͽ� ������ �ӵ��� ������� ������ ���� �ֵ��� �մϴ�.
                    Vector3 windDirection = transform.up;
                    Vector3 windForce = windDirection * power * Time.fixedDeltaTime;

                    // �÷��̾��� CharacterController�� ���� ã�� Move �޼��带 ȣ���մϴ�.
                    CharacterController controller = player.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.Move(windForce);
                    }
                }
            }
        }
    }
}
