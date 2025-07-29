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
    [SerializeField] private AvatarDistanceTrackerUI trackerUI;
   // [SerializeField] private AvatarDistanceTrackerUI distanceTrackerUI;
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


        localAvatar.Jump();
        localAvatar.AddForce(force);
        localAvatar.airControl = 0.03f;
        StartCoroutine(Deploy());

    }



    IEnumerator Deploy()
    {
        yield return new WaitForSeconds(1.3f);
        localAvatar.velocity = Vector3.zero;
        // parachute.mesh.enabled = true;
        SpatialBridge.actorService.localActor.avatar.EquipAttachment(AssetType.EmbeddedAsset, parachuteID);
        cam.thirdPersonOffset = new Vector3(0, 4, -10);
        // localAvatar.AddForce(new Vector3(2, 1f, 0));
        // localAvatar.fallingGravityMultiplier = 0.02f;
        localAvatar.gravityMultiplier = 0.02f;
        localAvatar.onLanded += LocalAvatar_onLanded;
    }
    private void LocalAvatar_onLanded()
    {
        localAvatar.airControl = 1;
        localAvatar.fallingGravityMultiplier = 1;
        localAvatar.gravityMultiplier = 1;
        SpatialBridge.actorService.localActor.avatar.ClearAttachments();
        // parachute.mesh.enabled = false;
        cam.thirdPersonOffset = defaultCamOffset;
        // distanceTrackerUI.calculate = true;
        trackerUI.EnableTracker();

    }


    public void SetParachuteID(string id)
    {
        parachuteID = id;
    }
    

}
