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
    [SerializeField] private Transform[] yachtSpots;
    [SerializeField] private Transform[] farmSpots;
    [SerializeField] private Transform[] cafeSpots;
    [SerializeField] private Transform[] moonSpots;
    [SerializeField] private Transform[] gateSpots;
    [SerializeField] private Transform[] helipadSpots;
    [SerializeField] private Transform[] danceStageSpots;
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
    [SerializeField] private AudioSource confetiAudio;
    [SerializeField] private AudioSource sixpStandAudio;
    [SerializeField] private ParticleSystem confetiVFX;
    [SerializeField] private AudioManager audioManager;




    [SerializeField] private string bFirework;
    [SerializeField] private string bShiled;
    [SerializeField] private string bPlaybirthday;






    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    // Remote event ID for "add score" requests
    private const byte SetPlaceEvent = 2;
    private const byte SetMarathonEvent = 3;
    private const byte ShowScoreboardEvent = 5;
    private const byte SetConfetiEvent = 6;
    private const byte ResetMarathonEvent = 11;
    private const byte LocateYachtEvent = 12;
    private const byte LocateFarmEvent = 13;
    private const byte LocateCafeEvent = 14;
    private const byte LocateMoonEvent = 15;
    private const byte LocateGateEvent = 16;
    private const byte LocateHelipadEvent = 17;
    private const byte LocateDanceEvent = 18;
    private const byte shutdownDanceMusicEvent = 19;
    private const byte EnableDanceMusicEvent = 20;
    private const byte playDanceMusicEvent = 21;





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


        if (args.eventID == SetMarathonEvent)
        {
            SetMarathon();
        }

        if (args.eventID == ShowScoreboardEvent)
        {
            scoreboardManager.ShowScoreboard();
        }

        if (args.eventID == SetConfetiEvent)
        {
            SetConfeti();
        }

        if (args.eventID == ResetMarathonEvent)
        {
            ResetLeaderBoard();
        }

        if (args.eventID == LocateYachtEvent)
        {
            LocatePlayersInYacht();
        }

        if (args.eventID == LocateFarmEvent)
        {
            LocatePlayersInFarm();
        }

        if(args.eventID == LocateCafeEvent)
        {
             LocatePlayersInCafe();
        }

        if(args.eventID == LocateMoonEvent)
        {
             LocatePlayersInMoon();
        }

        if(args.eventID == LocateGateEvent)
        {
            LocatePlayersInGate();
        }

        if(args.eventID == LocateHelipadEvent)
        {
            LocatePlayersInHelipad();
        }

        if(args.eventID == LocateDanceEvent)
        {
            LocatePlayersInDanceStage();
        }

        if(args.eventID == shutdownDanceMusicEvent)
        {
            ShutdownDanceMusic();
        }

        if(args.eventID == EnableDanceMusicEvent)
        {
            EnableDanceMusic();
        }

        if(args.eventID == playDanceMusicEvent)
        {
            PlayDanceMusic();
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

    public void LocatePlayersInYacht()
    {
        int v = Random.Range(0, yachtSpots.Length);
        localAvatar.position = yachtSpots[v].position;
    }

    public void LocatePlayersInFarm()
    {
        int v = Random.Range(0, farmSpots.Length);
        localAvatar.position = farmSpots[v].position;
    }

    public void LocatePlayersInCafe()
    {
        int v = Random.Range(0, cafeSpots.Length);
        localAvatar.position = cafeSpots[v].position;
    }

    public void LocatePlayersInMoon()
    {
        int v = Random.Range(0, moonSpots.Length);
        localAvatar.position = moonSpots[v].position;
    }

    public void LocatePlayersInGate()
    {
        int v = Random.Range(0, gateSpots.Length);
        localAvatar.position = gateSpots[v].position;
    }


    public void LocatePlayersInHelipad()
    {
        int v = Random.Range(0, helipadSpots.Length);
        localAvatar.position = helipadSpots[v].position;
    }


    public void LocatePlayersInDanceStage()
    {
        int v = Random.Range(0, danceStageSpots.Length);
        localAvatar.position = danceStageSpots[v].position;
    }

    public void SetMarathon()
    {
        trackerUI.EnableTracker();
        StartCoroutine(MarathonCounter());
         
    }

    public void ResetLeaderBoard()
    {
        source.Stop();
        timer.StopAndHideCountdown();
        scoreboardManager.RequestReset();
        finishLine.hasWorked = false;
        finishLine.inMarathon = false;
    }

    public void SetConfeti()
    {
        confetiVFX.Play();
        confetiAudio.Play();
        sixpStandAudio.Play();
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


    public void ShutdownDanceMusic()
    {
        audioManager.ForceShutdownDanceMusic();
    }

    public void EnableDanceMusic()
    {
        audioManager.ForceEnableDanceMusic();
    }

    public void PlayDanceMusic()
    {
        audioManager.PlayDanceMusic();
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

    public void UIConfeti()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(SetConfetiEvent);

    }

    public void UIResetLeaderBoard() 
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(ResetMarathonEvent);
    }


    public void UILocateYacht()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateYachtEvent);
    }

    public void UILocateFarm()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateFarmEvent);
    }

    public void UILocateCafe()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateCafeEvent);

    }

    public void UILocateMoon()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateMoonEvent);
    }

    public void UILocateGate()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateGateEvent);
    }

    public void UILocateHelipad()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateHelipadEvent);
    }

    public void UILocateDanceStage()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(LocateDanceEvent);
    }

    public void UIShutdownDanceMusic()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(shutdownDanceMusicEvent);

    }

    public void UIEnableDanceMusic()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(EnableDanceMusicEvent);

    }

    public void UIPlayDanceMusic()
    {
        _syncObject.TakeoverOwnership();
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(playDanceMusicEvent);

    }

}

