using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DaricToMoonManager : MonoBehaviour
{
    public VideoPlayer player;
    public Animator animator;

    public bool start;

    private void Update()
    {

        
    }


    public void PlayVideo()
    {
        player.Stop();
        player.Play();
        
    }


}
