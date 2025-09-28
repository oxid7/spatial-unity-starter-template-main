using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelfieCamera : MonoBehaviour, IAvatarInputActionsListener
{
    public GameObject camera;
    public Vector3 offset;
    public GameObject canvas;
    public GameObject ui;
    private IAvatar localAvatar => SpatialBridge.actorService.localActor?.avatar;
    private ICameraService cam => SpatialBridge.cameraService;
    private Quaternion startRot;


    private Vector3 camDefaultOffset;
    private float camDefaultZoom;
    public void Start()
    {
        startRot = transform.rotation;
        transform.position = localAvatar.GetAvatarBoneTransform(HumanBodyBones.RightHand).position + offset; 
        transform.SetParent(localAvatar.GetAvatarBoneTransform(HumanBodyBones.Spine));
        camDefaultOffset = cam.thirdPersonOffset;
        camDefaultZoom = cam.zoomDistance;
    }



    public void Spawn()
    {
        // transform.position = localAvatar.position + offset;
        //  camera.SetActive(true);
       
        StartCoroutine(Play());
        
    }

    public void DeSpawn()
    {


        cam.thirdPersonOffset = camDefaultOffset;
        cam.zoomDistance = camDefaultZoom;
        SpatialBridge.cameraService.ClearTargetOverride();
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "selfieend", true, false);
        StartCoroutine(StopAction());
        camera.SetActive(false);
    }



    IEnumerator StopAction()
    {
        yield return new WaitForSeconds(2.5f);
        SpatialBridge.inputService.ReleaseInputCapture(this);
    }
    IEnumerator Play()
    {
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "selfie2nd", false,false);
        cam.thirdPersonOffset = new Vector3(0, 0.1f, 3.5f);
        cam.zoomDistance = camDefaultZoom;
        yield return new WaitForSeconds(4.3f);
        SpatialBridge.inputService.StartAvatarInputCapture(true, true, true, true, this);
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "selfiestuck", false, true);
        transform.position = localAvatar.GetAvatarBoneTransform(HumanBodyBones.RightHand).position + offset;
        transform.SetParent(localAvatar.GetAvatarBoneTransform(HumanBodyBones.Spine));
        camera.SetActive(true);
    }

    public void OnAvatarMoveInput(InputPhase inputPhase, Vector2 inputMove)
    {
       // throw new System.NotImplementedException();
       
    }

    public void OnAvatarJumpInput(InputPhase inputPhase)
    {
       // throw new System.NotImplementedException();
    }

    public void OnAvatarSprintInput(InputPhase inputPhase)
    {
       // throw new System.NotImplementedException();
    }

    public void OnAvatarActionInput(InputPhase inputPhase)
    {
       // throw new System.NotImplementedException();
    }

    public void OnAvatarAutoSprintToggled(bool on)
    {
       // throw new System.NotImplementedException();
    }

    public void OnInputCaptureStarted(InputCaptureType type)
    {
       // throw new System.NotImplementedException();
    }

    public void OnInputCaptureStopped(InputCaptureType type)
    {
       // throw new System.NotImplementedException();
    }
}
