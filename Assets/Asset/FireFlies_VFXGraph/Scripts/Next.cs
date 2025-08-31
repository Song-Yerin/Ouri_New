using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Next : MonoBehaviour
{
    public Text vfxName;
    public GameObject[] objects;
    int currntObject = 0;

    private void Start()
    {
        objects[currntObject].SetActive(true);
        vfxName.text = objects[currntObject].name.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextObject();
        }
    }
    public void NextObject()
    {
        objects[currntObject].SetActive(false);
        if (currntObject < objects.Length-1)
        {
            currntObject += 1;
            
        }
        else
        {
            currntObject = 0;
        }

        objects[currntObject].SetActive(true);
        vfxName.text = objects[currntObject].name.ToString();
    }
}
