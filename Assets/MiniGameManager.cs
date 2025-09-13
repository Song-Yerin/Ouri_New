using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager _instance;

    public bool GameStart = false;
    public GameObject player;

    private void Awake()
    {
        // 싱글톤 등록
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject); // 씬 이동 시 유지
    }


    public void StartGame()
    {
        GameStart = true;

        // if (!UIState.CursorShown)
        // {
        //     Cursor.visible = false;
        //     Cursor.lockState = CursorLockMode.Locked;
        // }

        player.GetComponent<Animator>().enabled = true;
        player.GetComponent<CreatureMover>().enabled = true;
        player.GetComponent<MovePlayerInput>().enabled = true;
        player.GetComponent<CursorManager>().enabled = true;
    }
}


