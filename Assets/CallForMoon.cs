using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallForMoon : MonoBehaviour
{

    public Animator moonAnimate;

    public bool activeMoon;


    private void Update()
    {
        
        moonAnimate.enabled = activeMoon;
    }




}
