using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public ParachuteManager parachute;

    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;
    [SerializeField] private Vector3 force;
    public void Jumper()
    {

        localAvatar.velocity = Vector3.zero;
        Invoke("JumpAfter", 0.01f);
        
    }

    public void JumpAfter()
    {
        parachute.trackerUI.DisableTracker();

        // localAvatar.Jump();
        // localAvatar.maxJumpCount = 0;
        localAvatar.velocity = force;
        localAvatar.airControl = 0.005f;

        // call the Parachte

        StartCoroutine(CallDeploy());
    }
    IEnumerator CallDeploy()
    {
        yield return new WaitForSeconds(3f);
        parachute.StartCoroutine(parachute.Deploy(false));
    }

}
