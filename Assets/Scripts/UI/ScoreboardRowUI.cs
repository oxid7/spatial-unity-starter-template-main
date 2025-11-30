using TMPro;
using UnityEngine;

public class ScoreboardRowUI : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI caloriesText;
    public TextMeshProUGUI stepsText;
    public TextMeshProUGUI rankText;
    public void SetData(PlayerScoreEntry entry, int rank)
    {
        usernameText.text = entry.username;
        dateText.text = entry.date;
        caloriesText.text = entry.calories;
        stepsText.text = entry.steps.ToString();
        rankText.text = rank.ToString();
    }
}
