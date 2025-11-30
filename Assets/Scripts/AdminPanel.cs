using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using TMPro;
public class AdminPanel : MonoBehaviour
{
    [SerializeField] private GameObject adminUI;
    [SerializeField] private List<string> admins = new();

   
    private void Start()
    {
        string user = SpatialBridge.actorService.localActor.username;
        user = user.ToLower();
        if(admins.Contains(user))
        {
            adminUI.SetActive(true);
        }

    }



}
