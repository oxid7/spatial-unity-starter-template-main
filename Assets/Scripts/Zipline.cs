using UnityEngine;
using SpatialSys.UnitySDK; // make sure you have the Spatial SDK namespace

public class Zipline : MonoBehaviour
{
    [Header("Zipline Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Ride Settings")]
    public float initialSpeed = 2f;
    public float acceleration = 0.5f;
    public float maxSpeed = 8f;
    public bool autoDetachAtEnd = true;
    public float rotationLerpSpeed = 5f;
    public Vector3 endForce;

    [Header("Offset Settings")]
    public Vector3 rideOffset = new Vector3(0, -1f, 0);
    public Vector3 handlerOffset = new Vector3(0, -1f, 0);

    public bool isRiding = false;
    public ParachuteManager parachuteManager;
    private float currentSpeed;
    private float rideProgress; // 0=start, 1=end


    // Shortcut to local player avatar
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    private float defaultGravity;
    private float defaultAirControl;

    private void Start()
    {
        defaultAirControl = localAvatar.airControl;
        defaultGravity = localAvatar.gravityMultiplier;

    }

    private void Update()
    {
        if (isRiding && localAvatar != null)
        {
         //   handler.gameObject.SetActive(true);
            localAvatar.gravityMultiplier = 0;
            localAvatar.airControl = 0;
            localAvatar.velocity = Vector3.zero;

            //set animation
            /*
            localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Zipline_Idle", true, true);
            localAvatar.EquipAttachment(AssetType.EmbeddedAsset, "Handler");*/

            // Fraction rate
            float fraction = Random.Range(0.9f, 1.1f);

            // accelerate gradually
            currentSpeed += acceleration * fraction * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, initialSpeed, maxSpeed);

            // move along zipline
            float distance = Vector3.Distance(startPoint.position, endPoint.position);
            rideProgress += (currentSpeed / distance) * Time.deltaTime;
            rideProgress = Mathf.Clamp01(rideProgress);

            Vector3 basePos = Vector3.Lerp(startPoint.position, endPoint.position, rideProgress);

            // add offset in rope's local orientation
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            Quaternion ropeRotation = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 offsetPos = basePos + ropeRotation * rideOffset;
            Vector3 offsetHanlderPos = basePos + ropeRotation * handlerOffset;
            // apply to Spatial avatar
            localAvatar.position = offsetPos;
            // rotate to face movement direction
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
                localAvatar.rotation = Quaternion.Lerp(localAvatar.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
                
            }

            // reached the end
            if (rideProgress >= 1f && autoDetachAtEnd)
            {
                DetachPlayer();
            }
        }
    }

    public void AttachPlayer()
    {
        rideProgress = 0f;
        currentSpeed = initialSpeed;
        isRiding = true;

        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Zipline_Idle", true, true);
        localAvatar.EquipAttachment(AssetType.EmbeddedAsset, "Handler");

    }

    public void DetachPlayer()
    {
        isRiding = false;
        localAvatar.StopEmote();

        localAvatar.AddForce(endForce);
        localAvatar.gravityMultiplier = defaultGravity;
        localAvatar.airControl = defaultAirControl;
      //  handler.gameObject.SetActive(false);
        localAvatar.ClearAttachments();
        parachuteManager.LocalAvatarSetup();
        
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawSphere(startPoint.position, 0.2f);
            Gizmos.DrawSphere(endPoint.position, 0.2f);
        }
    }

  
    public void ShowAnimation()
    {
        localAvatar.EquipAttachment(AssetType.EmbeddedAsset, "Handler");
        localAvatar.PlayEmote(AssetType.EmbeddedAsset, "Zipline_Idle", true, true);
    }
}
