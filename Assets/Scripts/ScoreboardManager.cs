using SpatialSys.UnitySDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

[Serializable]
public class PlayerScoreEntry
{
    public string username;
    public string date;       // "2025-11-13 18:24:53"
    public string calories;   // formatted text, e.g. "10.6"
    public int steps;
}

[Serializable]
public class ScoreboardData
{
    public List<PlayerScoreEntry> entries = new List<PlayerScoreEntry>();
}

public class ScoreboardManager : MonoBehaviour
{
    public Variables syncedVar;     // Visual Scripting variables (for now)
    private const string ScoreJsonKey = "scoreJson";
    private const string MarathonJsonKey = "MarathonLeaderboard";
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";   // must match your TimeHelper format

    // Remote event ID for "add score" requests
    private const byte ADD_SCORE_EVENT_ID = 1;
    private const byte ADD_MARATHON_EVENT_ID = 4;
    private const byte RESET_MARATHON_EVENT_ID = 81;

    public TextMeshProUGUI scoreboardLog;

    // UI table
    public Transform rowsParent;         // parent with VerticalLayoutGroup
    public GameObject rowPrefab;         // prefab with ScoreboardRowUI
    public GameObject scoreboardPanel;
    public TMP_InputField usernameSearchFiled;

    // ------------------------------------------------------------------
    // LIFECYCLE: subscribe/unsubscribe to remote events
    // ------------------------------------------------------------------
    private void OnEnable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent += HandleRemoteEvent;
    }

    private void OnDisable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent -= HandleRemoteEvent;
    }

    // This runs on ALL clients when a remote event is raised.
    // We only actually mutate the scoreboard on the MASTER client.
    private void HandleRemoteEvent(NetworkingRemoteEventArgs args)
    {
        /*
        if (args.eventID != ADD_SCORE_EVENT_ID)
            return;*/

        //   if (args.eventID != ADD_MARATHON_EVENT_ID) return;
        if (args.eventID == ADD_MARATHON_EVENT_ID)
        {
            // Only master client should write the scoreboard JSON
            if (!SpatialBridge.networkingService.isMasterClient)
                return;

            // We expect: username (string), date (string), calories (float), steps (int)
            string username = (string)args.eventArgs[0];
            string duration = (string)args.eventArgs[1];  //change it to date
            float calories = (float)args.eventArgs[2];
            int steps = (int)args.eventArgs[3];

            AddOrUpdatePlayerScore(username, duration, calories, steps); //change it to date
        }

        if(args.eventID == RESET_MARATHON_EVENT_ID)
        {

            // Only master client should write the scoreboard JSON
            if (!SpatialBridge.networkingService.isMasterClient)
                return;

            ResetMarathon();
        }
    }


    // ------------------------------------------------------------------
    // PUBLIC ENTRY POINT: call this from other scripts
    // ------------------------------------------------------------------
    // Everyone calls THIS. It sends a network event to all clients.
    // Only master client processes it and actually updates the JSON.
    // NOTE: date is now passed IN from the caller (you can use TimeHelper there).
    public void RequestAddScore(string username, string date, float calories, int steps)
    {
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(
            ADD_SCORE_EVENT_ID,
            username,
            date,
            calories,
            steps
        );
    }



    public void RequestAddMarathon(string username, string duration, float calories, int steps)
    {
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(
            ADD_MARATHON_EVENT_ID,
            username,
            duration,
            calories,
            steps
        );
    }


    public void RequestReset()
    {
        SpatialBridge.networkingService.remoteEvents.RaiseEventAll(RESET_MARATHON_EVENT_ID);
    }

    public void ResetMarathon()
    {
        syncedVar.declarations.Set(MarathonJsonKey, "");
    }

    // ------------------------------------------------------------------
    // WRITE: add entry (MASTER CLIENT ONLY, via remote event)
    // ------------------------------------------------------------------
    // Do NOT call this directly from other scripts. Use RequestAddScore.
    public void AddOrUpdatePlayerScore(string username, string date, float calories, int steps)
    {
        // 1) Read current JSON
        string json = "";
        if (syncedVar.declarations.IsDefined(MarathonJsonKey))
        {
            json = syncedVar.declarations.Get<string>(MarathonJsonKey);
        }

        // 2) JSON -> object
        ScoreboardData data = string.IsNullOrEmpty(json)
            ? new ScoreboardData()
            : JsonUtility.FromJson<ScoreboardData>(json);

        // format calories once, as text (1 decimal)
        string caloriesText = Math.Round(calories, 1, MidpointRounding.AwayFromZero)
                                   .ToString("F1", CultureInfo.InvariantCulture);

        var newEntry = new PlayerScoreEntry
        {
            username = username,
            date = date,         // 👈 comes from the client
            calories = caloriesText,
            steps = steps
        };

        data.entries.Add(newEntry);

        // 4) object -> JSON
        string newJson = JsonUtility.ToJson(data, true);

        // 5) Save back into synced variable
        syncedVar.declarations.Set(MarathonJsonKey, newJson);
        if (scoreboardLog != null)
            scoreboardLog.text = newJson;
    }

    // ------------------------------------------------------------------
    // HELPERS: parsing / loading
    // ------------------------------------------------------------------
    private DateTime ParseDate(string dateStr)
    {
        if (DateTime.TryParseExact(
                dateStr,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt))
        {
            return dt;
        }

        // If parsing fails, push it to the very beginning
        return DateTime.MinValue;
    }

    private float ParseCalories(string caloriesStr)
    {
        if (string.IsNullOrEmpty(caloriesStr))
            return 0f;

        if (float.TryParse(
                caloriesStr,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float value))
        {
            return value;
        }

        return 0f;
    }

    private ScoreboardData LoadScoreboardData()
    {
        string json = "";

        if (syncedVar.declarations.IsDefined(MarathonJsonKey)) // change into ScoreJsonKey
        {
            json = syncedVar.declarations.Get<string>(MarathonJsonKey);
        }

        if (string.IsNullOrEmpty(json))
            return new ScoreboardData();

        ScoreboardData data = JsonUtility.FromJson<ScoreboardData>(json);
        return data ?? new ScoreboardData();
    }

    private void BuildRows(List<PlayerScoreEntry> entriesToShow)
    {
        if (rowsParent == null || rowPrefab == null)
            return;

        // Clear old rows
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        // Build new rows
        foreach (var entry in entriesToShow)
        {
            var rowObj = Instantiate(rowPrefab, rowsParent);
            var rowUI = rowObj.GetComponent<ScoreboardRowUI>();
            rowUI.SetData(entry, entriesToShow.IndexOf(entry)+1);
        }
    }

    // ------------------------------------------------------------------
    // READ: full scoreboard
    // ------------------------------------------------------------------
    public void RefreshScoreboardUI()
    {
        ScoreboardData data = LoadScoreboardData();
        if (data.entries == null)
            return;

        var list = new List<PlayerScoreEntry>(data.entries);

        // Sort by calories (highest first)
        list.Sort((a, b) =>
        {
            float calA = ParseCalories(a.calories);
            float calB = ParseCalories(b.calories);
            return calB.CompareTo(calA); // bigger first
        });

        BuildRows(list);
    }

    public void ShowScoreboard()
    {
        RefreshScoreboardUI();           // make sure data is fresh

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);
    }


    public void ShowUserboard()
    {
        var list = GetBestEntryPerUser();
        if(list == null) return;

        BuildRows(list);

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);

    }
    public void HideScoreboard()
    {
        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(false);
    }

    // ------------------------------------------------------------------
    // READ: filter by username
    // ------------------------------------------------------------------
    public void SearchForUser()
    {
        ShowScoreboardForUsername(usernameSearchFiled.text);
    }

    public void ShowScoreboardForUsername(string usernameFilter)
    {
        if (string.IsNullOrWhiteSpace(usernameFilter))
        {
            // if nothing typed, just show full scoreboard
            ShowScoreboard();
            return;
        }

        ScoreboardData data = LoadScoreboardData();
        if (data.entries == null)
            return;

        // filter by username (case-insensitive, partial match)
        var filtered = data.entries.FindAll(e =>
            !string.IsNullOrEmpty(e.username) &&
            e.username.IndexOf(usernameFilter, StringComparison.OrdinalIgnoreCase) >= 0
        );

        // sort filtered by date (newest first)
        filtered.Sort((a, b) => ParseDate(b.date).CompareTo(ParseDate(a.date)));

        BuildRows(filtered);

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);
    }

    // ------------------------------------------------------------------
    // READ: one "best" row per user
    // ------------------------------------------------------------------
    public List<PlayerScoreEntry> GetBestEntryPerUser()
    {
        ScoreboardData data = LoadScoreboardData();

        var bestByUser = new Dictionary<string, PlayerScoreEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in data.entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.username))
                continue;

            if (!bestByUser.TryGetValue(entry.username, out var currentBest))
            {
                bestByUser[entry.username] = entry;
                continue;
            }

            DateTime entryDate = ParseDate(entry.date);
            DateTime currentDate = ParseDate(currentBest.date);

            if (entryDate > currentDate)
            {
                
                bestByUser[entry.username] = entry;
            }
            else if (entryDate == currentDate)
            {
              
                float entryCal = ParseCalories(entry.calories);
                float currentCal = ParseCalories(currentBest.calories);

                if (entryCal > currentCal)
                {
                    bestByUser[entry.username] = entry;
                }
            }
        }

        var resultList = new List<PlayerScoreEntry>(bestByUser.Values);

        // Sort final list: newest date first, then highest calories
        resultList.Sort((a, b) =>
        {
            int dateCompare = ParseDate(b.date).CompareTo(ParseDate(a.date));
            if (dateCompare != 0)
                return dateCompare;

            float calA = ParseCalories(a.calories);
            float calB = ParseCalories(b.calories);

            return calB.CompareTo(calA);   // highest calories first
        });

        return resultList;
    }
}
