using UnityEngine;
using UnityEngine.UI; // For regular UI (Text)
using SpatialSys.UnitySDK;
using TMPro;
public class AvatarDistanceTrackerUI : MonoBehaviour
{
    private IAvatar localAvatar => SpatialBridge.actorService.localActor.avatar;

    public GameObject watchButton;
    

    public TextMeshProUGUI vStep; // Assign this in the Inspector
    public TextMeshProUGUI vCal;
    public TextMeshProUGUI information;
    public TMP_InputField emailFiled;
    private Vector3 lastPosition;
    private float totalDistanceTravelled = 0f;

    private bool isInitialized = false;
    private float timer = 0f;
    public float updateInterval = 1f; // seconds
    public bool calculate = false;



    public float step;
    public float cal;
    public string email;
    private void Start()
    {
        calculate = false;
    }

    void Update()
    {
        if (!calculate) 
        {
            // distanceText.enabled = false;
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
           // distanceText.enabled = true;
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
            // Step 1: Estimate steps (assume average step length = 0.8 meters)
            float stepLength = 0.8f; // meters per step
            int estimatedSteps = Mathf.RoundToInt(totalDistanceTravelled / stepLength);

            // Step 2: Estimate kcal burned (1000 steps ≈ 40 kcal)
            float kcalPerStep = 40f / 1000f;
            float estimatedKcal = estimatedSteps * kcalPerStep;

            // display or store values
            cal = estimatedKcal;
            step = estimatedSteps;


            timer = 0f; // reset timer
        }
    }


    public void SetEmail()
    {
        email = emailFiled.text;
    }
    public void EnableTracker()
    {
        watchButton.SetActive(true);
        calculate = true;
    }

    public void ShowWatch()
    {


        vStep.text = step.ToString();
        vCal.text = cal.ToString();
        information.text = email;


    }
   

    public void Convert()
    {
        cal = 0;
        step = 0;
        totalDistanceTravelled = 0;
        vStep.text = step.ToString();
        vCal.text = cal.ToString();
    }
   
}
