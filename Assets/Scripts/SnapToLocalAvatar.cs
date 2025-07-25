using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToLocalAvatar : MonoBehaviour
{
    public Vector3 offset;
   
    public MeshRenderer mesh;
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    private void Start()
    {
        mesh.enabled = false;
    }
    private void Update()
    {
        transform.parent.position = localAvatar.position + offset;
        transform.parent.rotation = localAvatar.rotation;
       

    }


    

}
