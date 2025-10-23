using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAttackModule : MonoBehaviour
{
    private Transform player;
    private PlayerFeedbackManager playerFeedback;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.TryGetComponent<PlayerFeedbackManager>(out playerFeedback);

            if (playerFeedback != null)
            {
                playerFeedback.CarDamage(1);
            }
        }
    }

}
