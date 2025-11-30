using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using Unity.VisualScripting;
public class SyncState : MonoBehaviour
{
    [SerializeField] private SpatialSyncedObject _syncObject;
    [SerializeField] private Variables vars;
    [SerializeField] private Fireworks fireworks;
    [SerializeField] private GameObject shiled;
    [SerializeField] private HBD hBD;
    [SerializeField] private Transform[] marathonSpots;
    [SerializeField] private GameObject backColider;
    [SerializeField] private GameObject frontColider;
    [SerializeField] private GameObject endLineTrigger;
    [SerializeField] private FinishLine finishLine;
    [SerializeField] private GameObject boostButton;
    [SerializeField] private AvatarDistanceTrackerUI trackerUI;
    [SerializeField] private Animator[] UIanimators;
    [SerializeField] private CountdownTimerCoroutine timer;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip music;
    [SerializeField] private ScoreboardManager scoreboardManager;
    



    [SerializeField] private string bFirework;
    [SerializeField] private string bShiled;
    [SerializeField] private string bPlaybirthday;






    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    // Remote event ID for "add score" requests
    private const byte SetPlaceEvent = 2;
    private const byte SetMarathonEvent = 3;
    private const byte ShowScoreboardEvent = 5;




    private void OnEnable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent += HandleRemoteEvent;
    }

    private void OnDisable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent -= HandleRemoteEvent;
    }


    private void HandleRemoteEvent(NetworkingRemoteEventArgs args)
    {


        if (args.eventID == SetPlaceEvent)
        {
            SetPlayersInPlace();
        }


        if(args.eventID == SetMarathonEvent)
        {
            SetMarathon();
        }

        if(args.eventID == ShowScoreboardEvent)
        {
            scoreboardManager.ShowScoreboard();
        }
        
    }

    public void PlayFireworks()
    {
        fireworks.Play();

        if (_syncObject.isLocallyOwned)
        {
            Invoke("ServerFireworkOff", 0.3f);
        }

        else
        {
            vars.declarations.Set(bFirework, false);
        }
        

    }

    public void PlayBirthday()
    {
        hBD.PlayAnimation();
        if (_syncObject.isLocallyOwned) 
        {
            Invoke("ServerBirthdayOff", 0.3f);
        }

        else
        {
            vars.declarations.Set(bPlaybirthday, false);
        }
        
    }

    public void SetPlayersInPlace()
    {
        backColider.SetActive(true);
        frontColider.SetActive(true);
        endLineTrigger.SetActive(true);
        finishLine.inMarathon = true;
        boostButton.SetActive(false);
        int v = Random.Range(0, marathonSpots.Length);
        localAvatar.position = marathonSpots[v].position;
    }


    public void SetMarathon()
    {
        trackerUI.EnableTracker();
        StartCoroutine(MarathonCounter());
         
    }


    IEnumerator MarathonCounter()
    {
        for (int i = 0; i < UIanimators.Length; i++)
        {
            UIanimators[i].Play("In");
            UIanimators[i].gameObject.GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(1);
        }
        frontColider.SetActive(false);
        timer.EnableAndStartCountdown();
        trackerUI.ResetTracker();
        source.clip = music;
        source.loop = true;
        source.Play();
    }
    public void ServerBirthdayOff()
    {
        vars.declarations.Set(bPlaybirthday, false);
    }

    public void ServerFireworkOff()
    {
        vars.declarations.Set(bFirework, false);
    }
    private void Update()
    {

        shiled.SetActive((bool)vars.declarations.Get(bShiled));

        if (_syncObject.isLocallyOwned) return;

       

        if((bool)vars.declarations.Get(bFirework) == true)
        {
            PlayFireworks();
        }

        if ((bool)vars.declarations.Get(bPlaybirthday) == true)
        {
            PlayBirthday();
        }

    }



    public void EndMarathon()
    {
        boostButton.SetActive(true);
        source.Stop();
        frontColider.SetActive(false);
        backColider.SetActive(false);
        

    }
    public void UIFirework()
    {
        _syncObject.TakeoverOwnership();
        vars.declarations.Set(bFirework, true);
        PlayFireworks();
    }

    public void UIBirhtday()
    {
        _syncObject.TakeoverOwnership();
        vars.declarations.Set(bPlaybirthday, true);
        PlayBirthday();
    }

    public void UIShiled(bool b)
    {
        _syncObject.TakeoverOwnership();
        vars.declarations.Set(bShiled, b);
    }

    public void UISetPlace()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(SetPlaceEvent);
    }
   
    public void UIInitiateMarathon()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(SetMarathonEvent);
    }

    public void UIShowScoreboardToAll()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(ShowScoreboardEvent);
    }
}
