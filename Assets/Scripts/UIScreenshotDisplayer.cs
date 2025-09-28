using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpatialSys.UnitySDK;
public class UIScreenshotDisplayer : MonoBehaviour
{
    [Tooltip("Drag your RawImage here")]

    public RawImage targetUI;
    public GameObject selfieUI;
    public Camera photoCamera;
    public SelfieCamera selfie;
    [SerializeField, Range(0f, 0.60f)]
    float sideCropPercent = 0.30f; // 30% by default, tweak in Inspector
    public ICameraService cameraService => SpatialBridge.cameraService;
    public IAvatar avatar => SpatialBridge.actorService.localActor.avatar;
    // Call this from your UI button’s OnClick()
    public void CaptureAndShowOnUI()
    {
        // StartCoroutine(CaptureCoroutine());
        avatar.PlayEmote(AssetType.EmbeddedAsset, "zplq82o78o4jkchczuur0fqh");
    }

    public void PlayAndTakePhoto()
    {

        //  StartCoroutine(CaptureCoroutine());
        selfie.Spawn();

    }

    public void Take()
    {
        StartCoroutine(TakePhoto());
    }

    IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Hide selfie UI before capture
        selfie.ui.SetActive(false);
        yield return new WaitForEndOfFrame();

        // Capture full screen
        Texture2D screenshot = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
        screenshot.Apply();

        // --- Crop percentage from left and right ---
        int cropX = Mathf.RoundToInt(screenWidth * sideCropPercent);
        int targetWidth = screenWidth - (cropX * 2);
        int targetHeight = screenHeight;

        Color[] pixels = screenshot.GetPixels(cropX, 0, targetWidth, targetHeight);
        Texture2D portrait = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        portrait.SetPixels(pixels);
        portrait.Apply();

        // Assign cropped portrait
        targetUI.texture = portrait;
        targetUI.enabled = true;

        // Restore UI
        selfie.ui.SetActive(true);
        selfie.DeSpawn();
        selfieUI.SetActive(true);
    }




    IEnumerator CaptureCoroutine()
    {

        avatar.PlayEmote(AssetType.EmbeddedAsset, "zplq82o78o4jkchczuur0fqh");

        yield return new WaitForSeconds(4.5f);

        // 1️⃣ Wait for the frame to finish drawing
        yield return new WaitForEndOfFrame();

        // 2️⃣ Figure out the actual backbuffer size
        int width = cameraService.scaledPixelWidth;
        int height = cameraService.scaledPixelHeight;

        // 3️⃣ Create a RenderTexture and a temp Camera
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        var go = new GameObject("TempCaptureCam");
        var cam = go.AddComponent<Camera>();

        // 4️⃣ Copy all settings & transform from Spatial’s main camera
        cameraService.CopyFromMainCamera(cam);

        // 5️⃣ Render into our RT
        cam.targetTexture = rt;
        cam.Render();

        // 6️⃣ Read pixels back into a Texture2D
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // 7️⃣ Clean up
        cam.targetTexture = null;
        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(rt);
        Destroy(go);

        // 8️⃣ Push it into your UI
        targetUI.texture = screenshot;
        targetUI.enabled = true;

        selfieUI.SetActive(true);

        /*
        // 9️⃣ (Optional) Match your UI aspect to avoid stretching
        float aspect = (float)width / height;
        RectTransform rtUI = targetUI.rectTransform;
        rtUI.sizeDelta = new Vector2(rtUI.sizeDelta.y * aspect, rtUI.sizeDelta.y);
        */
    }


    IEnumerator CaptureWithHand()
    {

        avatar.PlayEmote(AssetType.EmbeddedAsset, "zplq82o78o4jkchczuur0fqh");

        yield return new WaitForSeconds(4.5f);

        // 1️⃣ Wait for the frame to finish drawing
        yield return new WaitForEndOfFrame();

        // 2️⃣ Figure out the actual backbuffer size
        int width = cameraService.scaledPixelWidth;
        int height = cameraService.scaledPixelHeight;

        // 3️⃣ Create a RenderTexture and a temp Camera
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        var go = new GameObject("TempCaptureCam");
        var cam = go.AddComponent<Camera>();

        // 4️⃣ Copy all settings & transform from Spatial’s main camera
        cameraService.CopyFromMainCamera(cam);

        // 5️⃣ Render into our RT
        cam.targetTexture = rt;
        cam.Render();

        // 6️⃣ Read pixels back into a Texture2D
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // 7️⃣ Clean up
        cam.targetTexture = null;
        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(rt);
        Destroy(go);

        // 8️⃣ Push it into your UI
        targetUI.texture = screenshot;
        targetUI.enabled = true;

        selfieUI.SetActive(true);

        /*
        // 9️⃣ (Optional) Match your UI aspect to avoid stretching
        float aspect = (float)width / height;
        RectTransform rtUI = targetUI.rectTransform;
        rtUI.sizeDelta = new Vector2(rtUI.sizeDelta.y * aspect, rtUI.sizeDelta.y);
        */
    }
}
