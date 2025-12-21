using SpatialSys.UnitySDK;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DanceManager : MonoBehaviour
{
    [Serializable]
    public class EmoteEntry
    {
        public string name;
        public float lengthSeconds = 2f;
    }

    [SerializeField] private List<EmoteEntry> emotes = new();
    [SerializeField] private TextMeshProUGUI velocityText;

    [SerializeField] private float velocityThreshold = 8f;
    [SerializeField] private float betweenEmotesDelay = 1f;

    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    private Coroutine danceRoutine;
    private bool isDancing;

    private void Update()
    {
        if (velocityText != null)
            velocityText.text = localAvatar.velocity.ToString("F3");
    }


   

    public void PlayByIndex(int v)
    {
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, emotes[v].name);
    }
    public void StartDancing()
    {
        if (danceRoutine != null) StopCoroutine(danceRoutine);
        isDancing = true;
        danceRoutine = StartCoroutine(DanceLoop());
    }

    public void StopDancing()
    {
        localAvatar.StopEmote();
        isDancing = false;
        if (danceRoutine != null)
        {
            StopCoroutine(danceRoutine);
            danceRoutine = null;
        }
    }

    private IEnumerator DanceLoop()
    {
        if (emotes.Count == 0)
        {
            Debug.LogWarning("No emotes set.");
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        while (isDancing)
        {
            // Wait until idle
            while (isDancing && !IsIdle())
                yield return null;

            if (!isDancing) yield break;

            // Pick random emote entry
            var entry = emotes[UnityEngine.Random.Range(0, emotes.Count)];
            if (string.IsNullOrWhiteSpace(entry.name))
            {
                yield return null;
                continue;
            }

            // Play
            localAvatar.PlayEmote(AssetType.EmbeddedAsset, entry.name, false);

            // Wait for its length, but break if movement starts
            float length = Mathf.Max(0.01f, entry.lengthSeconds);
            float t = 0f;

            while (isDancing && t < length)
            {
                if (!IsIdle())
                {
                    localAvatar.StopEmote();
                    break;
                } 
                t += Time.deltaTime;
                yield return null;
            }

            if (betweenEmotesDelay > 0f)
                yield return new WaitForSeconds(betweenEmotesDelay);
        }
    }

    private bool IsIdle()
    {
        if(localAvatar.velocity.sqrMagnitude <= velocityThreshold)
        {
            return true;
        }

        return false;
    }
}
