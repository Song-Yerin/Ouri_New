using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] float bounceForce = 10f;

    [SerializeField] private float shakeMagnitude = 10f;               // 흔들림 진폭 (절댓값)
    [SerializeField] private float shakeFrequency = 4f;                // 흔들림 속도 (진동수)
    [SerializeField] private Vector3 shakeDirection = Vector3.forward; // Z축 방향으로 흔들기

    [SerializeField] private int Count = 2;
    [SerializeField] private AudioSource AudioSource;

    int CountOfJump;
    private Quaternion _initialRotation;



    private void OnEnable()
    {
        AudioSource = this.GetComponent<AudioSource>();
        CountOfJump = Count;

        _initialRotation = transform.localRotation;
        StartCoroutine(ShakeForever());
    }

    void Update()
    {
        if (CountOfJump <= 0)
        {
            // 오브젝트 비활성화
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ShakeForever()
    {
        //흔들리게

        while (true)
        {
            float angle = Mathf.Sin(Time.time * shakeFrequency * Mathf.PI * 2f) * shakeMagnitude;
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
    }

    private IEnumerator WaitForSecond()
    {
        AudioSource.Play();
        float waitTime = AudioSource.clip != null ? AudioSource.clip.length : 1f;
        yield return new WaitForSeconds(waitTime);

        CountOfJump--; // 사운드 재생 끝난 후 감소
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            StartCoroutine(WaitForSecond());

            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                CreatureMover playerController = other.GetComponent<CreatureMover>();
                if (playerController != null)
                {
                    playerController.Bounce(Vector3.up * bounceForce);
                }
            }
        }
    }

    
}



