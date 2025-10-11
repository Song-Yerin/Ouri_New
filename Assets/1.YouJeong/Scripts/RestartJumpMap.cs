using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartJumpMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ActivateInactiveChildren()
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"자식 오브젝트 활성화됨: {child.name}");
            }
        }
    }
}
