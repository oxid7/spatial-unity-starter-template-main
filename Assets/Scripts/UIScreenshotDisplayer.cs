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
        // 2️⃣ Figure out the actual backbuffer size
        int width = cameraService.scaledPixelWidth;
        int height = cameraService.scaledPixelHeight;

        // 3️⃣ Create a RenderTexture and a temp Camera
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        var go = new GameObject("TempCaptureCam");
        var cam = go.AddComponent<Camera>();

        // 4️⃣ Copy all settings & transform from Spatial’s main camera
       // cameraService.CopyFromMainCamera(cam);
        cam.transform.position = selfie.transform.position;
        cam.transform.rotation = selfie.transform.rotation;
        cam.fieldOfView = 80;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 400;

        selfie.canvas.SetActive(false);
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
        selfie.canvas.SetActive(true);
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
