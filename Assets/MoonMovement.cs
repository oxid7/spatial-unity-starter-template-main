using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonMovement : MonoBehaviour
{

    public Vector3 direction;
    public float speed;

    void Update()
    {
        transform.Rotate(direction * speed * Time.deltaTime );


    }
}
