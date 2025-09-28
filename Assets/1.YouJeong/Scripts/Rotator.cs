using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 18f; // 초당 회전

    void Update()
    {
        // Y축 기준으로 rotationSpeed만큼 초당 회전 - 로컬좌표계기준
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}
