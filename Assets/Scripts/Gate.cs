using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class Gate : MonoBehaviour
{
    public int requiredCal;
    public int convertableSIXP;
    public int remianingBatteries;
    public float resetTime;

    public TextMeshProUGUI remainingBatteryOne;
    public TextMeshProUGUI remainingBatteryTwo;

    public GameObject convertUI;
    public GameObject convertError;

    public AvatarDistanceTrackerUI trackerUI;

    private bool isAlive;

    private void Start()
    {
        isAlive = true;
      StartCoroutine(Countdown());
        

    }

    IEnumerator Countdown()
    {

        while(isAlive)
        {
            yield return new WaitForSeconds(resetTime);
            remianingBatteries = Random.Range(5, 20);
            remainingBatteryOne.text = "Available batteries : " + remianingBatteries.ToString();
            remainingBatteryTwo.text = "Available batteries : " + remianingBatteries.ToString();
            yield return null;
        }

    }


    public void Convert()
    {
        if (trackerUI.cal < requiredCal)
        {
            convertError.SetActive(true);
            StartCoroutine(TurnOffError());
            return;
        }
       
        else
        {
            convertError.SetActive(false);
            convertUI.SetActive(true);
            remianingBatteries--;
            remainingBatteryOne.text = "Available batteries : " + remianingBatteries.ToString();
            remainingBatteryTwo.text = "Available batteries : " + remianingBatteries.ToString();
        }
    }


    IEnumerator TurnOffError()
    {
        yield return new WaitForSeconds(7);
        convertError.SetActive(false);
    }
}
