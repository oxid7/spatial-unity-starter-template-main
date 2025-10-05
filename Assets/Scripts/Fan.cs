using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public Vector3 movement;
    public float speed;
    public Variables varS;
    public bool slowDown = false;

    private float initiateSpeed;
    private void Start()
    {
        initiateSpeed = speed;
    }
    public void Update()
    {
        if (!slowDown)
        {
            speed = initiateSpeed;
            transform.Rotate(movement * initiateSpeed * Time.deltaTime);
        }
        
        else if(speed > 0)
        {
            Slowdown();
        }

        else
        {
            this.enabled = false;
        }
    }


    public void Slowdown()
    {
        if (speed > 0)
        {
            speed -= (initiateSpeed / 2) * Time.deltaTime;
            transform.Rotate(movement * speed * Time.deltaTime);
        }
    }
}
