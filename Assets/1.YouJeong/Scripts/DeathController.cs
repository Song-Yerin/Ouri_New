using Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    [SerializeField] RestartJumpMap restartJumpMap;
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("1");

        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            Debug.Log("2");

            if (controller != null)
            {
                CreatureMover playerController = other.GetComponent<CreatureMover>();
                if (playerController != null)
                {
                    playerController.Respawn();
                    Debug.Log("3");
                }
            }
            restartJumpMap.ActivateInactiveChildren();
        }


    }
}
