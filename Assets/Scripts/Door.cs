using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private Transform target;
    [SerializeField] private float safeDistance;



    private bool isOpend;

    private void Start()
    {

        /*
        animator.Play("Open");
        isOpend = true;
        */
    }



    private void Update()
    {/*
        Debug.Log(Vector3.Distance(transform.position, target.position));

        if(Vector3.Distance(transform.position,target.position) < safeDistance)
        {
            if (!isOpend)
            {
                animator.Play("Open");
                isOpend = true;
            }

        }

        else 
        {
            if ( isOpend)
            {
                Debug.Log("wer");
                animator.Play("Close");
                isOpend = false;
            }
        }
        */
    }



    public void Open()
    {
        animator.Play("Open");
    }

    public void Close()
    {
        animator.Play("Close");
    }
}
