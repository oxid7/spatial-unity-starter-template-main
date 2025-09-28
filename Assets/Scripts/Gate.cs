using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Gate : MonoBehaviour
{
    public int requiredCal;
    public int convertableSIXP;
    public int remianingBatteries;
    public float resetTime;

    public TextMeshProUGUI remainingBatteryOne;
    public TextMeshProUGUI remainingBatteryTwo;

    public GameObject convertUI;
    public Button convertButton;
    public GameObject convertError;

    public AvatarDistanceTrackerUI trackerUI;

    private bool isAlive;



    private void OnEnable()
    {
      //  convertButton.onClick.AddListener(Convert);
    }
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

        /*
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
        
        VERSION No 1 */


        if (trackerUI.cal > requiredCal)
        {
            convertButton.gameObject.SetActive(true);
            return;
        }


    }




    public void ExitArea()
    {
        convertButton.gameObject.SetActive(false);
    }
    IEnumerator TurnOffError()
    {
        yield return new WaitForSeconds(7);
        convertError.SetActive(false);
    }


    private void OnDisable()
    {
      //  convertButton.onClick.RemoveListener(Convert);
    }
}
