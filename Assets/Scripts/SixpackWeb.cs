using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
public class SixpackWeb : MonoBehaviour
{
    [SerializeField] private string buyURL = "https://metamap.sixpackminer.io/";
    
    

    public void GoToBuy()
    {
       SpatialBridge.spaceService.OpenURL(buyURL);
    }
}
