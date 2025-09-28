using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float bounceForce = 10f;

    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
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



