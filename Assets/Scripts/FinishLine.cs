using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SpatialSys.UnitySDK;
public class FinishLine : MonoBehaviour
{
    [SerializeField] private ScoreboardManager scoreboardManager;
    [SerializeField] private CountdownTimerCoroutine countdown;
    [SerializeField] private AvatarDistanceTrackerUI trackerUI;
    [SerializeField] private TextMeshProUGUI finishText;
    private string username;
    private bool hasWorked = false;

    public bool inMarathon = false;
    private void Start()
    {
        username = SpatialBridge.actorService.localActor.username;
        hasWorked = false;
    }

    public void OnEnd()
    {
        if (hasWorked) return;
        if (!inMarathon) return;

        if(countdown.timeRemaining > 0)
        {
            scoreboardManager.RequestAddMarathon(username, ConvertTime(), trackerUI.cal, (int)trackerUI.step);
            finishText.text = "You finished the marathon with " + trackerUI.cal.ToString() +" calories.";
            finishText.gameObject.GetComponent<Animator>().Play("In");
            finishText.gameObject.GetComponent<AudioSource>().Play();
            
        }
        else
        {
            finishText.text = "You did not finish the marathon in the required time.";
            finishText.gameObject.GetComponent<Animator>().Play("In");
        }

        hasWorked = true;
        gameObject.SetActive(false);
        
    }


    public void Show()
    {
       scoreboardManager.ShowScoreboard();
    }

    public string ConvertTime()
    {
        int elapsed = countdown.startTimeInSeconds - countdown.timeRemaining;
        int minutes = elapsed / 60;
        int seconds = elapsed % 60;

        return string.Format("{0:00}:{1:00}", minutes, seconds);

    }
}
