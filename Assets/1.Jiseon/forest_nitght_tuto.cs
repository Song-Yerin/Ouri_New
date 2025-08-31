using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class forest_nitght_tuto : MonoBehaviour
{
    // Start is called before the first frame update

    public KeyHintSpawner keyHintSpawner;
    void Start()
    {
        keyHintSpawner.ShowKeyHint("E", "E 키를 눌러 대화를 시작하세요.", 13f);
    }

}
