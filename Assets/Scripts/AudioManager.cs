using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource announcerSource;
    public AudioSource musicSource;
    

    public AudioClip welcomeToMoon;
    public AudioClip sixpMusic;


    private float defaultMusicVol;
    private float defaultAnnouncerVol;


    private void Start()
    {
        defaultAnnouncerVol = announcerSource.volume;
        defaultMusicVol = musicSource.volume;


        
    }
    public void PlayWelcomeToTheMoon()
    {
        announcerSource.PlayOneShot(welcomeToMoon);

    }

    public void PlaySixpMusic()
    {
        if (musicSource.isPlaying) return;
        musicSource.volume = 0;
        musicSource.PlayOneShot(sixpMusic);
        StartCoroutine(SourceEffects(10f, 20, 10f, musicSource, defaultMusicVol));
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
