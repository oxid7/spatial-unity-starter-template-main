using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swimming : MonoBehaviour, IAvatarInputActionsListener
{
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;
    public bool isMoving;
    public bool setAnim;


    private bool isInWater = false;
    private Vector2 moveSet;
    private void Update()
    {

        if (isMoving)
        {
            if(!setAnim)
            {
                localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Swim", false, true);
                setAnim = true;
            }

            localAvatar.Move(moveSet, true);
           
        }

        else if(isInWater)
        {
            localAvatar.velocity = Vector3.zero;
          //  localAvatar.Move(Vector2.zero, false);
        }
    }

    public void GoSwimState()
    {
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Swim_Idle", false, true);
        SpatialBridge.inputService.StartAvatarInputCapture(true, true, true, true, this);
        isInWater = true;
    }

    public void ExitSwimState()
    {
        localAvatar.StopEmote();
        SpatialBridge.inputService.ReleaseInputCapture(this);
        isInWater = false;
        isMoving = false;
        setAnim = false;
        moveSet = Vector2.zero;
    }

    public void OnAvatarMoveInput(InputPhase inputPhase, Vector2 inputMove)
    {
        if (inputPhase == InputPhase.OnHold)
        {
            moveSet = inputMove;
            isMoving = true;
        }
        else if (inputPhase == InputPhase.OnReleased)
        { isMoving = false; setAnim = false; localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Swim_Idle", false, true); moveSet = Vector2.zero; }
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
      //  throw new System.NotImplementedException();
    }

    public void OnAvatarAutoSprintToggled(bool on)
    {
       // throw new System.NotImplementedException();
    }

    public void OnInputCaptureStarted(InputCaptureType type)
    {
      //  throw new System.NotImplementedException();
    }

    public void OnInputCaptureStopped(InputCaptureType type)
    {
      
    }
}
