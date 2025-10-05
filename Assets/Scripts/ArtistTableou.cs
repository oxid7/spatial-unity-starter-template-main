using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtistTableou : MonoBehaviour
{

    public ArtistUI manager;
    [SerializeField] private Sprite art;
    [SerializeField] private string artName;
    [SerializeField] private string artistName;
    [SerializeField] private string artDetails;
    [SerializeField] private string artistDetails;
    [SerializeField] private string instaID;
    [SerializeField] private string portfolioLink;

    
    public void Interact()
    {
        manager.ShowUI(art,artName,artistName,artDetails,artistDetails,instaID,portfolioLink);
    }

}
