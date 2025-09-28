#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraIconCaptureEditor : EditorWindow
{
    private Camera targetCamera;

    [MenuItem("Tools/Capture Camera Icon %#i")] // Ctrl+Shift+I shortcut
    public static void ShowWindow()
    {
        GetWindow<CameraIconCaptureEditor>("Camera Icon Capture");
    }

    private void OnGUI()
    {
        GUILayout.Label("Camera Icon Capture", EditorStyles.boldLabel);

        targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);

        if (GUILayout.Button("Capture 1920x1080 PNG"))
        {
            if (targetCamera == null)
            {
                Debug.LogError("Please assign a Camera to capture from.");
            }
            else
            {
                CaptureIcon(targetCamera);
            }
        }
    }

    private void CaptureIcon(Camera cam)
    {
        // Store old clear flags and background
        CameraClearFlags oldFlags = cam.clearFlags;
        Color oldBg = cam.backgroundColor;

        // Force transparent clear
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);

        // Setup render target with alpha
        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;
        cam.targetTexture = rt;
        cam.Render();

        // Copy to texture
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Restore camera settings
        cam.clearFlags = oldFlags;
        cam.backgroundColor = oldBg;

        // Save inside Assets
        string folderPath = "Assets/CapturedIcons";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "CameraIcon.png");
        File.WriteAllBytes(filePath, tex.EncodeToPNG());

        AssetDatabase.Refresh();
        Debug.Log("✅ Transparent icon saved to: " + filePath);
    }
}
#endif
