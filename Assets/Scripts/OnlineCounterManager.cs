using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
public class OnlineCounterManager : MonoBehaviour
{
    public Variables SharedVar;
    public SpatialSyncedObject syncedObject;
    public TextMeshProUGUI onlineCounterText;
    public int minOnlineUser = 3;


    private void Start()
    {
        UpdateUserCount();
    }
    private void OnEnable()
    {
        SpatialBridge.actorService.onActorJoined += AddToCount;
        SpatialBridge.actorService.onActorLeft += SubtractFromCount;
    }

    private void OnDisable()
    {
        SpatialBridge.actorService.onActorJoined -= AddToCount;
        SpatialBridge.actorService.onActorLeft -= SubtractFromCount;
    }


    public void AddToCount(ActorJoinedEventArgs args)
    {/*
        if(syncedObject.isMasterClientObject)
        {
            int currentUserCount = (int)SharedVar.declarations.Get("UserCount");
            currentUserCount++;
            SharedVar.declarations.Set("UserCount", currentUserCount);
            
        }
        */
        Invoke("UpdateUserCount", 0.5f);
    }

    public void SubtractFromCount(ActorLeftEventArgs args)
    {/*
        if (syncedObject.isMasterClientObject)
        {
            int currentUserCount = (int)SharedVar.declarations.Get("UserCount");
            currentUserCount--;
            if(currentUserCount > minOnlineUser)
            {
                SharedVar.declarations.Set("UserCount", currentUserCount);
            }
        }
        */
        Invoke("UpdateUserCount", 0.5f);

    }

    public void UpdateUserCount()
    {
        //  Debug.Log("Update UI here");

        int online = (int)SpatialBridge.actorService.actorCount + 1;
        onlineCounterText.text = online + " Online"; 
    }
}
