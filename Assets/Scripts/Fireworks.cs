using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireworks : MonoBehaviour
{
    [SerializeField] private ParticleSystem firework1;
    [SerializeField] private ParticleSystem firework2;
    [SerializeField] private ParticleSystem firework3;
    [SerializeField] private ParticleSystem firework4;


    public void Play()
    {
        StartCoroutine(Launch());
    }

    public IEnumerator Launch()
    {
        firework1.Play();
        yield return new WaitForSeconds(2);
        firework2.Play();
        yield return new WaitForSeconds(1.5f);
        firework3.Play();
        yield return new WaitForSeconds(3);
        firework4.Play();
        yield return new WaitForSeconds(3);
        firework1.Play();
        yield return new WaitForSeconds(1.5f);
        firework3.Play();
    }
}
