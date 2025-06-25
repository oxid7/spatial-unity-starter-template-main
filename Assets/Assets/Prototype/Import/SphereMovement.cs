using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMovement : MonoBehaviour
{
    public Vector3 movement;
    public float speed;



    public void Update()
    {
       transform.Rotate(movement * speed * Time.deltaTime);

    }
}
