using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpatialSys.UnitySDK;
using TMPro;
public class UIProfileInfo : MonoBehaviour
{
    [SerializeField] private string username;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private Image profileColor;
    [SerializeField] private RawImage profileImage;



    private void Start()
    {
        Invoke("GatherInfo", 5);
    }
    public void GatherInfo()
    {
        username = SpatialBridge.actorService.localActor.username;
        usernameText.text = username;
        var c = SpatialBridge.actorService.localActor.profileColor;
        if (QualitySettings.activeColorSpace == ColorSpace.Linear) c = c.gamma;
        profileColor.color = c;

        SpatialBridge.actorService.localActor.GetProfilePicture().SetCompletedEvent((request) =>
        {
            profileImage.texture = request.texture;
            

        });
    }


    Color GetSpatialUIMatchedProfileColor()
    {
        // 1) Start from Spatial profile color
        var c = SpatialBridge.actorService.localActor.profileColor;

        // 2) Make sure we're working in gamma when the project is Linear
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            c = c.gamma;

        // 3) Convert to HSV
        Color.RGBToHSV(c, out float h, out float s, out float v);

        // 4) Apply calibrated mapping (from your measurements)
        const float hueDeltaDeg = -14.93f;
        const float hueDelta = hueDeltaDeg / 360f;
        const float satScale = 0.87982832618f;
        const float valScale = 1.03097345133f;

        h = (h + hueDelta) % 1f; if (h < 0f) h += 1f;
        s = Mathf.Clamp01(s * satScale);
        v = Mathf.Clamp01(v * valScale);

        // 5) Back to RGB (keep original alpha)
        var outColor = Color.HSVToRGB(h, s, v);
        outColor.a = c.a;
        return outColor;
    }

}
