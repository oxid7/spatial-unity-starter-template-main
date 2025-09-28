using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class AuthClient
{
    // ---- Config ----
    private const string BaseUrl = "https://testnet.sixpackminer.com/api";
    private const int TimeoutSeconds = 15;

    // Build a "browser-like" User-Agent when we are NOT in WebGL.
    // In WebGL, the real browser UA will be sent automatically and you cannot override it.
    private static string BrowserUserAgent =>
#if UNITY_IOS
        "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1";
#elif UNITY_ANDROID
        "Mozilla/5.0 (Linux; Android 13; Mobile) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36";
#else
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
#endif

    // ---- Public API ----

    /// <summary>
    /// Step 1: Ask server to send a code to the email.
    /// </summary>
    public static IEnumerator RequestLoginCode(string email, System.Action<bool, string> onDone)
    {
        var url = $"{BaseUrl}/security/GetCode/login";
        var payload = JsonUtility.ToJson(new EmailOnly { email = email });
        yield return SendJsonPost(url, payload, (ok, bodyOrError, code) =>
        {
            onDone?.Invoke(ok, bodyOrError);
        });
    }

    /// <summary>
    /// Step 2: Submit email + code. Expect a token or session back.
    /// Returns raw body so you can parse based on the server schema.
    /// </summary>
    public static IEnumerator Login(string email, int code, System.Action<bool, string> onDone)
    {
        var url = $"{BaseUrl}/security/login";
        var payload = JsonUtility.ToJson(new EmailAndCode { email = email, code = code });
        yield return SendJsonPost(url, payload, (ok, bodyOrError, status) =>
        {
            onDone?.Invoke(ok, bodyOrError);
        });
    }

    // ---- Helpers ----

    private static IEnumerator SendJsonPost(string url, string json,
        System.Action<bool, string, long> onComplete)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = TimeoutSeconds;

            req.SetRequestHeader("Content-Type", "application/json");

            // Only set User-Agent when allowed (not WebGL).
            // Browsers forbid changing it; in WebGL you must NOT set it.
#if !UNITY_WEBGL
            req.SetRequestHeader("User-Agent", BrowserUserAgent);
#endif

            yield return req.SendWebRequest();

            bool ok = req.result == UnityWebRequest.Result.Success &&
                      (req.responseCode >= 200 && req.responseCode < 300);

            if (ok)
                onComplete?.Invoke(true, req.downloadHandler.text, req.responseCode);
            else
            {
                var err = string.IsNullOrEmpty(req.downloadHandler?.text)
                          ? req.error
                          : $"HTTP {req.responseCode}: {req.downloadHandler.text}";
                onComplete?.Invoke(false, err, req.responseCode);
            }
        }
    }

    [System.Serializable]
    private class EmailOnly { public string email; }

    [System.Serializable]
    private class EmailAndCode { public string email; public int code; }
}
