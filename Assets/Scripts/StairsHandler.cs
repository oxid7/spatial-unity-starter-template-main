using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
public class StairsHandler : MonoBehaviour
{
    [SerializeField] private Vector3 direction;
    [SerializeField] private float speed;
    public bool isInside;
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    public void Update()
    {
        if (!isInside) return;
        // localAvatar.velocity += direction * speed;
        localAvatar.position += direction * speed * Time.deltaTime;
    }


    public void PlayerInside()
    {
        isInside = true;
    }

    public void PlayerOutside()
    {
        isInside = false;
    }
}
