using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public float speed = 0f;

    [SerializeField] private bool rotateX;
    [SerializeField] private bool rotateY;
    [SerializeField] private bool rotateZ;

    private Vector3 currentAngle;

    void Start()
    {
        currentAngle = transform.eulerAngles;
    }

    void Update()
    {
        if (rotateX)
        {
            currentAngle.x += Time.deltaTime * speed * 100f;
        }
        if (rotateY)
        {
            currentAngle.y += Time.deltaTime * speed * 100f;
        }
        if (rotateZ)
        {
            currentAngle.z += Time.deltaTime * speed * 100f;
        }

        transform.eulerAngles = currentAngle;
    }
}
