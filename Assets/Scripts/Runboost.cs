using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpatialSys.UnitySDK;

public class Runboost : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private Image waitButton;
    [SerializeField] private float speed;
    [SerializeField] private float boostTime;
    [SerializeField] private float activateTime;
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    private float defaultWalkSpeed;
    private float defaultRunSpeed;
    private bool hasBoosted = false; 

    private void Start()
    {
        defaultWalkSpeed = localAvatar.walkSpeed;
        defaultRunSpeed = localAvatar.runSpeed;

    }
    public void Boost()
    {
        if (hasBoosted) return;
        localAvatar.runSpeed = localAvatar.runSpeed * speed;
        localAvatar.walkSpeed = localAvatar.walkSpeed * speed;
        waitButton.fillAmount = 1;
        waitButton.enabled = true;
        
        // ButtonTimer();
        StartCoroutine(ButtonTimer());
        Invoke("SetBack", boostTime);
        hasBoosted = true;
    }

    public void SetBack()
    {
        localAvatar.walkSpeed = defaultWalkSpeed;
        localAvatar.runSpeed = defaultRunSpeed;
       
    }

    private IEnumerator ButtonTimer()
    {
        yield return new WaitForSeconds(boostTime);
        while (waitButton.fillAmount > 0)
        {
            waitButton.fillAmount -= activateTime * Time.deltaTime;
            yield return null;  // wait for the next frame
        }

        waitButton.enabled = false;
        hasBoosted = false;
    }


}
