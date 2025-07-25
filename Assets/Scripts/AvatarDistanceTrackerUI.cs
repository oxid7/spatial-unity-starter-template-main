using UnityEngine;
using UnityEngine.UI; // For regular UI (Text)
using SpatialSys.UnitySDK;
using TMPro;
public class AvatarDistanceTrackerUI : MonoBehaviour
{
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    public TextMeshProUGUI distanceText; // Assign this in the Inspector

    private Vector3 lastPosition;
    private float totalDistanceTravelled = 0f;

    private bool isInitialized = false;
    private float timer = 0f;
    public float updateInterval = 1f; // seconds
    public bool calculate = false;

    private void Start()
    {
        calculate = false;
    }

    void Update()
    {
        if (!calculate) 
        {
            distanceText.enabled = false;
            return;
        }


        if (localAvatar == null)
            return;

        if (!isInitialized && !localAvatar.isGrounded) return;

        Vector3 currentPosition = localAvatar.position;
       
        if (!isInitialized)
        {
            lastPosition = currentPosition;
            isInitialized = true;
            distanceText.enabled = true;
            return;
        }

        float distanceThisFrame = Vector3.Distance(currentPosition, lastPosition);
        totalDistanceTravelled += distanceThisFrame;
        lastPosition = currentPosition;

        // Update timer
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            int distanceInt = Mathf.RoundToInt(totalDistanceTravelled);
            distanceText.text = "Traveled: " + distanceInt + " meters";

            timer = 0f; // reset timer
        }
    }

    public void Respawn()
    {
        localAvatar.Respawn();
    }
}
