using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class LandApi
{
    private const string BaseUrl = "https://testnet.sixpackminer.com/api";
    private const int TimeoutSeconds = 15;

    private static string BrowserUserAgent =>
#if UNITY_IOS
        "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1";
#elif UNITY_ANDROID
        "Mozilla/5.0 (Linux; Android 13; Mobile) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36";
#else
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
#endif

    [System.Serializable]
    private class LandId
    {
        public int id;
    }

    /// <summary>
    /// Fetch information about a land by ID.
    /// </summary>
    public static IEnumerator GetLandInfo(int landId, string bearerToken, System.Action<bool, string> onDone)
    {
        var url = $"{BaseUrl}/land/info";
        var payload = JsonUtility.ToJson(new LandId { id = landId });
        var bytes = Encoding.UTF8.GetBytes(payload);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = TimeoutSeconds;

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {bearerToken}");

#if !UNITY_WEBGL
            req.SetRequestHeader("User-Agent", BrowserUserAgent);
#endif

            yield return req.SendWebRequest();

            bool ok = req.result == UnityWebRequest.Result.Success &&
                      (req.responseCode >= 200 && req.responseCode < 300);

            if (ok)
                onDone?.Invoke(true, req.downloadHandler.text);
            else
            {
                var err = string.IsNullOrEmpty(req.downloadHandler?.text)
                          ? req.error
                          : $"HTTP {req.responseCode}: {req.downloadHandler.text}";
                onDone?.Invoke(false, err);
            }
        }
    }
}
