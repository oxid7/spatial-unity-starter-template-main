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
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";   // must match your TimeHelper format

    public TextMeshProUGUI scoreboardLog;

    // UI table
    public Transform rowsParent;         // parent with VerticalLayoutGroup
    public GameObject rowPrefab;         // prefab with ScoreboardRowUI
    public GameObject scoreboardPanel;
    public TMP_InputField usernameSearchFiled;

    // ----------------------
    // WRITE: add/update entry
    // ----------------------
    public void AddOrUpdatePlayerScore(string username, string date, float calories, int steps)
    {
        // 1) Read current JSON
        string json = "";
        if (syncedVar.declarations.IsDefined(ScoreJsonKey))
        {
            json = syncedVar.declarations.Get<string>(ScoreJsonKey);
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
            date = date,
            calories = caloriesText,
            steps = steps
        };

        data.entries.Add(newEntry);

        // 4) object -> JSON
        string newJson = JsonUtility.ToJson(data, true);

        // 5) Save back into synced variable
        syncedVar.declarations.Set(ScoreJsonKey, newJson);
        if (scoreboardLog != null)
            scoreboardLog.text = newJson;
    }

    // ----------------------
    // HELPERS: parsing / loading
    // ----------------------

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

        if (syncedVar.declarations.IsDefined(ScoreJsonKey))
        {
            json = syncedVar.declarations.Get<string>(ScoreJsonKey);
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
            rowUI.SetData(entry);
        }
    }

    // ----------------------
    // READ: full scoreboard
    // ----------------------

    public void RefreshScoreboardUI()
    {
        ScoreboardData data = LoadScoreboardData();
        if (data.entries == null)
            return;

        // sort by date (newest first)
        var list = new List<PlayerScoreEntry>(data.entries);
        list.Sort((a, b) => ParseDate(b.date).CompareTo(ParseDate(a.date)));

        BuildRows(list);
    }

    public void ShowScoreboard()
    {
        RefreshScoreboardUI();           // make sure data is fresh

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(true);
    }

    public void HideScoreboard()
    {
        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(false);
    }

    // ----------------------
    // READ: filter by username
    // ----------------------


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

    // ----------------------
    // NEW: one "best" row per user
    // ----------------------

    // For each username:
    //  - pick the entry with the newest date
    //  - if multiple on that date, pick the one with highest calories
    // Then return them sorted by date (newest first), then calories (highest first).
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
                // newer date wins
                bestByUser[entry.username] = entry;
            }
            else if (entryDate == currentDate)
            {
                // same date → pick higher calories
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

 