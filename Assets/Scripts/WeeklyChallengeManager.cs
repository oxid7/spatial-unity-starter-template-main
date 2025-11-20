using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Challenge
{
    public string Title;
    public string Description;
    public Sprite Icon;
}
public class WeeklyChallengeManager : MonoBehaviour
{
    public List<Challenge> Challenges;
}
