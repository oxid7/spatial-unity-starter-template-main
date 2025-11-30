using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using UnityEngine.Rendering;
public class MarathonItem : MonoBehaviour
{
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;
    public enum ObstacleType {SPEEDINCREASE, SPEEDDECREASE, FORCEBACK, FORCEFORWARD}

    public ObstacleType type;

    public float force;
    public float speedMultiplier;
    public float effectDuration;
    public GameObject mesh;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip sfx;

    private float defaultSpeed;
    private bool functional = true;
    private void Start()
    {
        defaultSpeed = localAvatar.runSpeed;
    }
    public void SpeedIncrease()
    {
        localAvatar.runSpeed = speedMultiplier * localAvatar.runSpeed;
        Invoke("ResetEffect", effectDuration);
        Invoke("ReFunction", effectDuration);

    }

    public void SpeedDecrease()
    {
        localAvatar.runSpeed = (1/speedMultiplier)*localAvatar.runSpeed;
        Invoke("ResetEffect", effectDuration);
        Invoke("ReFunction", effectDuration);
    }

    public void ForceForward()
    {
        localAvatar.AddForce(localAvatar.GetAvatarBoneTransform(HumanBodyBones.Hips).transform.parent.parent.forward * force);
        Invoke("ReFunction", effectDuration);
    }

    public void ForceBackward()
    {
        localAvatar.AddForce(localAvatar.GetAvatarBoneTransform(HumanBodyBones.Hips).transform.parent.parent.forward * -force);
        Invoke("ReFunction", effectDuration);
    }


    public void ResetEffect()
    {
        localAvatar.runSpeed = defaultSpeed;
    }

    public void ReFunction()
    {
        mesh.SetActive(true);
        functional = true;
    }
    public void Interact()
    {
        if (!functional) return;
        switch(type)
        {
            case ObstacleType.SPEEDINCREASE:
                SpeedIncrease(); break;
                case ObstacleType.SPEEDDECREASE:
                SpeedDecrease(); break;
                case ObstacleType.FORCEBACK:
                ForceBackward(); break;
                case ObstacleType.FORCEFORWARD:
                ForceForward(); break;
        }

        source.PlayOneShot(sfx);
        mesh.SetActive(false);
        functional = false;
    }

}
