using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ArtistUI : MonoBehaviour
{
    public GameObject UIPanel;
    [SerializeField] private Image art;
    [SerializeField] private TextMeshProUGUI artName;
    [SerializeField] private TextMeshProUGUI artistName;
    [SerializeField] private TextMeshProUGUI artDetails;
    [SerializeField] private TextMeshProUGUI artistDetails;
    [SerializeField] private TextMeshProUGUI instaID;
    [SerializeField] private string portfolioLink;
    [SerializeField] private string instagramLink;
    



    public void OpenInsta()
    {
        instagramLink = "https://www.instagram.com/" + instaID.text;
        SpatialBridge.spaceService.OpenURL(instagramLink);
    }


    public void OpenPortfolio()
    {
        SpatialBridge.spaceService.OpenURL(portfolioLink);
    }



    public void ShowUI(Sprite artImage,string artname, string artistname, string artdetails, string artistdetails, string instaid, string portfoliolink)
    {
        art.sprite = artImage;
        artName.text = artname;
        artistName.text = artistname;
        artDetails.text = artdetails;
        artistDetails.text = artistdetails;
        instaID.text = instaid;
        portfolioLink = portfoliolink;

        UIPanel.SetActive(true);


    }


}
