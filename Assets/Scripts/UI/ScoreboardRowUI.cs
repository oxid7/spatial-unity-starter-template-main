using TMPro;
using UnityEngine;

public class ScoreboardRowUI : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI caloriesText;
    public TextMeshProUGUI stepsText;

    public void SetData(PlayerScoreEntry entry)
    {
        usernameText.text = entry.username;
        dateText.text = entry.date;
        caloriesText.text = entry.calories;
        stepsText.text = entry.steps.ToString();
    }
}
