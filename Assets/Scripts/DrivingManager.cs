using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
public class DrivingManager : MonoBehaviour
{
    public List<SpatialInteractable> cars = new List<SpatialInteractable>();
    public List <Spatial_SCC> carControllers = new();

    public void DisableCars()
    {
        foreach(var car in carControllers)
        {
            car.DisableCar();
        }

    }

    public void EnableCars()
    {
        foreach (var car in carControllers)
        {
            car.EnableCar();
        }
    }
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
