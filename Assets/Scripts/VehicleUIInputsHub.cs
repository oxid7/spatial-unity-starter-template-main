using UnityEngine;
using SpatialSys.UnitySDK;

public class VehicleUIInputsHub : MonoBehaviour
{
    [Header("Wire this")]
    public Spatial_SCC vehicle;   // your object that implements IVehicleInputActionsListener

    // Button states (set by VehicleUIHoldButton)
    [HideInInspector] public bool throttleHeld;
    [HideInInspector] public bool reverseHeld;
    [HideInInspector] public bool steerLeftHeld;
    [HideInInspector] public bool steerRightHeld;

    // Previous states for transitions
    bool prevThrottle, prevReverse;
    int prevSteerDir; // -1 left, 0 none, +1 right

    void Update()
    {
        if (!vehicle) return;

        // ----------- STEER -----------
        int steerDir = 0;
        if (steerLeftHeld && !steerRightHeld) steerDir = -1;
        else if (steerRightHeld && !steerLeftHeld) steerDir = +1;
        else steerDir = 0; // both or none -> neutral

        if (steerDir != prevSteerDir)
        {
            // release previous
            if (prevSteerDir != 0)
                vehicle.OnVehicleSteerInput(InputPhase.OnReleased, Vector2.zero);

            // press new
            if (steerDir != 0)
                vehicle.OnVehicleSteerInput(InputPhase.OnPressed, new Vector2(steerDir, 0f));
        }
        else if (steerDir != 0)
        {
            vehicle.OnVehicleSteerInput(InputPhase.OnHold, new Vector2(steerDir, 0f));
        }
        prevSteerDir = steerDir;

        // ----------- THROTTLE / REVERSE -----------
        // (If both are held, they cancel = neutral. Change this if you want one to win.)
        bool throttleActive = throttleHeld && !reverseHeld;
        bool reverseActive = reverseHeld && !throttleHeld;

        // throttle transitions
        if (throttleActive != prevThrottle)
        {
            if (prevThrottle) vehicle.OnVehicleThrottleInput(InputPhase.OnReleased, 0f);
            if (throttleActive) vehicle.OnVehicleThrottleInput(InputPhase.OnPressed, 1f);
        }
        else if (throttleActive)
        {
            vehicle.OnVehicleThrottleInput(InputPhase.OnHold, 1f);
        }
        prevThrottle = throttleActive;

        // reverse transitions
        if (reverseActive != prevReverse)
        {
            if (prevReverse) vehicle.OnVehicleReverseInput(InputPhase.OnReleased, 0f);
            if (reverseActive) vehicle.OnVehicleReverseInput(InputPhase.OnPressed, 1f);
        }
        else if (reverseActive)
        {
            vehicle.OnVehicleReverseInput(InputPhase.OnHold, 1f);
        }
        prevReverse = reverseActive;
    }
}

