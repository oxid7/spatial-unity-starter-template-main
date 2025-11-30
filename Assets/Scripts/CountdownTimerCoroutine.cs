using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownTimerCoroutine : MonoBehaviour
{
    [Header("Timer Settings")]
    public int startTimeInSeconds = 180; // 3 minutes

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    public int timeRemaining;
    public SyncState syncState;
    private Coroutine countdownRoutine;

    private void Awake()
    {
        // Make sure the text is hidden at the beginning (optional)
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the timer text UI and starts the countdown.
    /// Call this when you want the countdown to begin.
    /// </summary>
    public void EnableAndStartCountdown()
    {
        if (timerText == null)
        {
            Debug.LogWarning("CountdownTimerCoroutine: timerText is not assigned.");
            return;
        }

        timerText.gameObject.SetActive(true);   // Show UI

        // Stop old coroutine if it's already running
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        timeRemaining = startTimeInSeconds;
        UpdateTimerText();
        countdownRoutine = StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);

            timeRemaining--;
            UpdateTimerText();
        }

        // Ensure we end exactly at 00:00
        timeRemaining = 0;
        UpdateTimerText();

        OnTimerFinished();
    }

    /// <summary>
    /// Called automatically when we reach 00:00.
    /// Disables the countdown and the text UI.
    /// </summary>
    private void OnTimerFinished()
    {
        countdownRoutine = null;
        timeRemaining = 0;
        syncState.EndMarathon();
        // Hide the UI text when finished
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        // If you want to do other things when finished, do it here:
        // e.g. trigger game over, next wave, etc.
        // Debug.Log("Timer finished!");
    }

    /// <summary>
    /// Manually stop the countdown and hide the UI (optional).
    /// </summary>
    public void StopAndHideCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    private void UpdateTimerText()
    {
        int minutes = timeRemaining / 60;
        int seconds = timeRemaining % 60;

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
