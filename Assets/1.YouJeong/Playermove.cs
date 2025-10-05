using UnityEngine;

public class Playermove : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 2f;

    private bool move = false;

    void Update()
    {
        if (move)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    public void MovePlayer()
    {
        move = true;
    }
}

