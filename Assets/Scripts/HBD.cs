using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HBD : MonoBehaviour
{
    public Animator animator;
    public AudioSource source;
    public AudioClip clip;
    public GameObject cake; 



    public void PlayAnimation()
    {
        if (source.isPlaying) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dark")) return;
        animator.Play("Dark");
        Invoke("ShowCake", 6);
        Invoke("PlaySound", 6.5f);
    }

    public void PlaySound()
    {
        source.PlayOneShot(clip);
    }

    public void ShowCake()
    {
        cake.SetActive(true);
       
    }


}
