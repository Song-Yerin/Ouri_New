using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SucceedModule : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has reached the end point!");
            other.GetComponent<StartChasingModule>().StopChase();


        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
