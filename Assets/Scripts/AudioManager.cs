using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource announcerSource;
    public AudioSource musicSource;
    public AudioSource danceStagaSource;
    

    public AudioClip welcomeToMoon;
    public AudioClip sixpMusic;


    private bool playDance = true;
    private float defaultMusicVol;
    private float defaultAnnouncerVol;

    private Coroutine danceCoroutine;

    private void Start()
    {
        defaultAnnouncerVol = announcerSource.volume;
        defaultMusicVol = musicSource.volume;
        playDance = true;

        
    }

    public void ForceShutdownDanceMusic()
    {
        if (danceCoroutine != null)
        {
            StopCoroutine(danceCoroutine);
        }

        danceStagaSource.Stop();
        playDance = false;
    }

    public void ForceEnableDanceMusic()
    {
        playDance = true;
    }

    
    public void PlayWelcomeToTheMoon()
    {
        announcerSource.PlayOneShot(welcomeToMoon);

    }

    public void StopWelcomeToTheMoon()
    {
        announcerSource.Stop();
    }

    public void PlaySixpMusic()
    {
        if (musicSource.isPlaying) return;
        musicSource.volume = 0;
        musicSource.PlayOneShot(sixpMusic);
        StartCoroutine(SourceEffects(10f, 20, 10f, musicSource, defaultMusicVol));
    }


    public void PlayDanceMusic()
    {
        if(!playDance) return;
        if(danceStagaSource.isPlaying) return;
        danceStagaSource.volume = 0;
        danceStagaSource.loop = true;
        danceStagaSource.Play();
        danceCoroutine = StartCoroutine(SourceEffectsFadeIn(5, danceStagaSource, 1));

    }

    public void StopDanceMusic()
    {
        if (!playDance) return;
        StopCoroutine(danceCoroutine);
        danceStagaSource.Stop();
    }

 

    IEnumerator SourceEffectsFadeIn(float fadeIn, AudioSource source, float defaultVol)
    {
        // Fade In
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, defaultVol, t / fadeIn);
            yield return null;
        }


    }

    IEnumerator SourceEffects(float fadeIn, float duration, float fadeOut, AudioSource source, float defaultVol)
    {
        // Fade In
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, defaultVol, t / fadeIn);
            yield return null;
        }

        // Stay at full volume
        yield return new WaitForSeconds(duration);

        // Fade Out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(defaultVol, 0f, t / fadeOut);
            yield return null;
        }

        source.Stop();
    }

}
