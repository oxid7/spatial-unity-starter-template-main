using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
public class ParachuteManager : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector3 force;
    [SerializeField] private string parachuteID;
    public AvatarDistanceTrackerUI trackerUI;
    [SerializeField] private Transform[] teleportPoints;
    [SerializeField] private GameObject Ui;
    [SerializeField] private AudioManager audio;
    // [SerializeField] private AvatarDistanceTrackerUI distanceTrackerUI;

    private bool hasAlreadyLanded = false;
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    private ICameraService cam => SpatialBridge.cameraService;

    private Vector3 defaultCamOffset;
    private void Start()
    {
        defaultCamOffset = cam.thirdPersonOffset;
        // cam.forceFirstPerson = true;
    }
    public void Jumper()
    {

        hasAlreadyLanded = false;
        localAvatar.Jump();
        localAvatar.AddForce(force);
        localAvatar.airControl = 0.015f;
        int id = Random.Range(1, 3);
        switch (id)
        {
            case 1:
                parachuteID = "1";
                break;
            case 2: parachuteID = "2"; break;
        }

        StartCoroutine(Deploy(true));
        
    }


    public void FastLanding()
    {

        int v = Random.Range(0, teleportPoints.Length);
        localAvatar.position = teleportPoints[v].position;


        localAvatar.airControl = 1;
        localAvatar.fallingGravityMultiplier = 1;
        localAvatar.gravityMultiplier = 1;
        SpatialBridge.actorService.localActor.avatar.ClearAttachments();
        // parachute.mesh.enabled = false;
        cam.thirdPersonOffset = defaultCamOffset;
        // distanceTrackerUI.calculate = true;
       // trackerUI.EnableTracker();
    }
    public IEnumerator Deploy(bool sound)
    {
        yield return new WaitForSeconds(1.3f);
        localAvatar.velocity = Vector3.zero;
        // parachute.mesh.enabled = true;
        SpatialBridge.actorService.localActor.avatar
            .EquipAttachment(AssetType.EmbeddedAsset, parachuteID);
        cam.thirdPersonOffset = new Vector3(0, 4, -10);
        // localAvatar.AddForce(new Vector3(2, 1f, 0));
        // localAvatar.fallingGravityMultiplier = 0.02f;
        localAvatar.gravityMultiplier = 0.05f;
        localAvatar.onLanded += LocalAvatar_onLanded;
        Ui.SetActive(true);
        if (sound) 
        {
            audio.PlayWelcomeToTheMoon();
        }
        
    }



    
    public void LocalAvatar_onLanded()
    {
        if (hasAlreadyLanded) return;
        localAvatar.airControl = 1;
        localAvatar.fallingGravityMultiplier = 1;
        localAvatar.gravityMultiplier = 1;
        SpatialBridge.actorService.localActor.avatar.ClearAttachments();
        // parachute.mesh.enabled = false;
        cam.thirdPersonOffset = defaultCamOffset;
        // distanceTrackerUI.calculate = true;
        trackerUI.EnableTracker();
        Ui.SetActive(false);
        localAvatar.maxJumpCount = 2;
        hasAlreadyLanded = true;
    }


    public void LocalAvatarSetup()
    {
        cam.thirdPersonOffset = defaultCamOffset;
        // distanceTrackerUI.calculate = true;
        trackerUI.EnableTracker();
        Ui.SetActive(false);
    }

    public void SetParachuteID(string id)
    {
        parachuteID = id;
    }
    

}
