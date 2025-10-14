using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class jumpMap2 : MonoBehaviour
{
    [SerializeField] PlayableDirector PlayableDirector;
    [SerializeField] GameObject camObject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            camObject.SetActive(true);
            PlayableDirector.Play();
        }
    }
}
