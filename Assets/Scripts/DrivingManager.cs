using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
public class DrivingManager : MonoBehaviour
{
    public List<SpatialInteractable> cars = new List<SpatialInteractable>();

    public void DisableAllInteractbles()
    {
        foreach (var interactable in cars)
        {
            interactable.gameObject.SetActive(false);
        }

    }


    public void EnableAllInteractables()
    {
        foreach (var interactable in cars)
        {
            interactable.gameObject.SetActive(true);
        }
    }
    
}
