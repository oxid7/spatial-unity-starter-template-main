using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using Unity.VisualScripting;
using UnityEngine.Events;
public class Spatial_SCC : MonoBehaviour, IVehicleInputActionsListener
{

    public SCC_InputProcessor inputProcessor;
    public Transform seat;
    public Transform cameraTarget;
    public GameObject UIControls;
    public Variables varS;
    public UnityEvent onCarActive;
    
    public Fan fan;

    public Vector3 initialPosition;
    public Vector3 initialRotation;

    public UnityEvent OnDriveStart;

    private SpatialSyncedObject _syncedObject;
    private SCC_Inputs _inputs = new SCC_Inputs();
    private bool _isDriving = false;

    private const VehicleInputFlags INPUT_FLAGS = VehicleInputFlags.Steer1D | VehicleInputFlags.Throttle | VehicleInputFlags.Reverse | VehicleInputFlags.PrimaryAction;
    
    private void Awake()
    {
        _syncedObject = GetComponent<SpatialSyncedObject>();
        


        // UpdateShouldBeDriving();
        // _syncedObject.onOwnerChanged += HandleOwnerChanged;
        //  SpatialBridge.spaceContentService.onSceneInitialized += HandleSceneInitialized;

    }



    public void SetController(VehicleUIInputsHub inputsHub)
    {
        inputsHub.vehicle = this;
    }
    public void DriveTheCar()
    {

        if ((bool)varS.declarations.Get("canBeDrived") == false) return;
        if ((bool)varS.declarations.Get("hasDriver")) return;
        _syncedObject.TakeoverOwnership();
        UpdateShouldBeDriving();
        OnDriveStart.Invoke();
    }

    public void DisableCar()
    {
        _syncedObject.TakeoverOwnership();
        varS.declarations.Set("canBeDrived", false);
    }


    public void EnableCar()
    {
        _syncedObject.TakeoverOwnership();
        varS.declarations.Set("canBeDrived", true);
    }
    private void OnDestroy()
    {
        SpatialBridge.inputService.ReleaseInputCapture(this);
        if (_isDriving)
            StoppedDriving();
    }

    private void Update()
    {
        if (_syncedObject.isLocallyOwned)
        {
            inputProcessor.OverrideInputs(_inputs);
            if(transform.position.y < 19.4)
            {
                // in sea 
                
                if(inputProcessor.inputs.throttleInput > 0 )
                {
                    fan.enabled = true;
                    fan.slowDown = false;
                }


                else if( fan.slowDown == false)
                {
                    fan.slowDown = true;
                }

            }

            else if (fan.slowDown == false)
            {
                fan.slowDown = true;
            }
        }
    }

    public void StartDriving()
    {
        SpatialBridge.inputService.StartVehicleInputCapture(INPUT_FLAGS, null, null, this);
        // UIControls.SetActive(true);
        onCarActive.Invoke();
        
    }

    public void StopDriving()
    {
        SpatialBridge.inputService.ReleaseInputCapture(this);
        Destroy(gameObject);
    }

    private void HandleOwnerChanged(int newOwner)
    {
        UpdateShouldBeDriving();
    }

    private void HandleSceneInitialized()
    {
        UpdateShouldBeDriving();
    }


    public void ResetPositionWithPlayer()
    {
        if (_syncedObject.isLocallyOwned)
        {
            transform.position = initialPosition;
            transform.eulerAngles = initialRotation;
        }
    }
    private void UpdateShouldBeDriving()
    {
        if (_syncedObject.isLocallyOwned)
        {
            if (!_isDriving)
            {
                StartDriving();
            }
        }
        else
        {
            if (_isDriving)
            {
                StopDriving();
            }
        }
    }

    private void StartedDriving()
    {
        SpatialBridge.cameraService.SetTargetOverride(cameraTarget, SpatialCameraMode.Vehicle);
        SpatialBridge.actorService.localActor.avatar.Sit(seat);
        _inputs.steerInput = 0;
        _inputs.handbrakeInput = 0;
        _inputs.throttleInput = 0;
        _inputs.brakeInput = 0;
        _isDriving = true;
        varS.declarations.Set("hasDriver", true);
    }

    public void StoppedDriving()
    {
        
        SpatialBridge.cameraService.ClearTargetOverride();
        SpatialBridge.actorService.localActor.avatar.Stand();
        SpatialBridge.actorService.localActor.avatar.position = transform.position + new Vector3(0, 1, -4);
        _inputs.handbrakeInput = 1;
        _inputs.throttleInput = 0;
        _inputs.brakeInput = 0;
        inputProcessor.OverrideInputs(_inputs);
       
        _isDriving = false;

       // UIControls.SetActive(false);

        Invoke("ReleaseInput", 0.5f);
        Invoke("Test", 2);
    }

    public void Test()
    {
        varS.declarations.Set("hasDriver", false);
    }
    public void ReleaseInput()
    {
        SpatialBridge.inputService.ReleaseInputCapture(this);
        

    }

#region IVehicleInputActionsListener

    public void OnInputCaptureStarted(InputCaptureType type)
    {
        StartedDriving();
    }

    public void OnInputCaptureStopped(InputCaptureType type)
    {
        StoppedDriving();
    }

    public void OnVehicleSteerInput(InputPhase inputPhase, Vector2 inputSteer)
    {
        _inputs.steerInput = inputSteer.x;
    }

    public void OnVehicleThrottleInput(InputPhase inputPhase, float inputThrottle)
    {
        _inputs.throttleInput = inputThrottle;
        

    }

    public void OnVehicleReverseInput(InputPhase inputPhase, float inputReverse)
    {
        _inputs.brakeInput = inputReverse;
      
    }

    public void OnVehiclePrimaryActionInput(InputPhase inputPhase)
    {
        _inputs.handbrakeInput = inputPhase != InputPhase.OnReleased ? 1 : 0;
    }

    public void OnVehicleSecondaryActionInput(InputPhase inputPhase)
    {
    }

    public void OnVehicleExitInput()
    {
        StopDriving();
    }

#endregion
}
