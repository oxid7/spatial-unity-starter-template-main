using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SixpackApiClient : MonoBehaviour
{
    // -------- Optional Singleton for easy access --------
    public static SixpackApiClient Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
       
    }

    // -------- Settings --------
    [Header("API")]
    public string baseUrl = "https://testnet.sixpackminer.com/api";
    public string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36";

    [Header("Auth (current state, kept only in memory)")]
    public string email = "you@example.com";
    public string oneTimeCode = "";         // last code used
    [TextArea] public string token;         // bearer token after login
    public int coachId;
    public string status;
    public string walletAddress;
    public bool authenticatorEnabled;

    [Header("HTTP")]
    public int timeoutSeconds = 20;
    public bool verboseLogs = true;

    [Header("UI")]
    public TextMeshProUGUI emailField;
    public TextMeshProUGUI coachIDFiled;
    public TextMeshProUGUI firstLetter;

    // -------- Inspector test buttons --------
    [ContextMenu("1) Get Login Code (uses inspector email)")]
    public void Ctx_GetCode() => StartCoroutine(RequestLoginCode(r =>
    {
        if (!r.IsSuccess) Debug.LogError("[GetCode] " + r);
        else Debug.Log("[GetCode] " + r.Data?.message);
    }));

    [ContextMenu("2) Login (uses inspector email/code)")]
    public void Ctx_Login() => LoginWithCode(email, oneTimeCode, r =>
    {
        if (!r.IsSuccess) Debug.LogError("[Login] " + r);
        else Debug.Log($"[Login] OK. coachId={coachId}, status={status}, token set={(!string.IsNullOrEmpty(token))}");
    });

    [ContextMenu("3) CanUpload (id=6)")]
    public void Ctx_CanUpload() => StartCoroutine(CanUpload(6, r =>
    {
        if (!r.IsSuccess) Debug.LogError("[CanUpload] " + r);
        else Debug.Log($"[CanUpload] canUpload={r.Data.canUpload} success={r.Data.success} msg={r.Data.message} remaining={r.Data.remaining}/{r.Data.limit}");
    }));

    // =======================
    // PUBLIC ENTRY POINTS (call from other scripts)
    // =======================

    /// <summary>
    /// Request a login code for a specific email. Keeps email in this component and invokes your callback.
    /// </summary>
    public void RequestLoginCodeFor(string username, Action<ApiResult<GetCodeResponse>> callback)
    {
        email = username; // sync local state
        StartCoroutine(RequestLoginCode(callback));
    }

    /// <summary>
    /// Log in with email + code. Saves token & user data as in-memory variables in this component.
    /// </summary>
    public void LoginWithCode(string username, string code, Action<ApiResult<LoginResponse>> callback)
    {
        StartCoroutine(LoginWithCodeRoutine(username, code, callback));
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(token);

    // =======================
    // High-level API flows
    // =======================

    private IEnumerator RequestLoginCode(Action<ApiResult<GetCodeResponse>> onDone)
    {
        var body = new GetCodeRequest { email = email };
        yield return PostJson<GetCodeRequest, GetCodeResponse>("/security/GetCode/login", body, withAuth: false, onDone: onDone);
    }

    private IEnumerator LoginWithCodeRoutine(string username, string code, Action<ApiResult<LoginResponse>> onDone)
    {
        // keep inspector fields in sync
        email = username;
        oneTimeCode = code;

        ApiResult<LoginResponse> final = null;

        // Try numeric first if code looks numeric; else send string
        if (int.TryParse(code, out var numeric))
        {
            var bodyNum = new LoginRequestNumber { email = username, code = numeric };
            yield return PostJson<LoginRequestNumber, LoginResponse>("/security/login", bodyNum, withAuth: false, onDone: r => final = r);

            // If 400 and looks like type issue, retry as string
            if (!final.IsSuccess && final.StatusCode == 400 && MightBeTypeMismatch(final.RawBody))
            {
                var bodyStr = new LoginRequestString { email = username, code = code };
                yield return PostJson<LoginRequestString, LoginResponse>("/security/login", bodyStr, withAuth: false, onDone: r => final = r);
            }
        }
        else
        {
            var bodyStr = new LoginRequestString { email = username, code = code };
            yield return PostJson<LoginRequestString, LoginResponse>("/security/login", bodyStr, withAuth: false, onDone: r => final = r);

            // If 400 + type hint, retry numeric if possible
            if (!final.IsSuccess && final.StatusCode == 400 && MightBeTypeMismatch(final.RawBody) && int.TryParse(code, out var num2))
            {
                var bodyNum = new LoginRequestNumber { email = username, code = num2 };
                yield return PostJson<LoginRequestNumber, LoginResponse>("/security/login", bodyNum, withAuth: false, onDone: r => final = r);
            }
        }

        // Save result in-memory on success
        if (final.IsSuccess && final.Data != null)
        {
            token = final.Data.token;
            coachId = final.Data.coachId;
            status = final.Data.status;
            walletAddress = final.Data.walletAddress;
            authenticatorEnabled = final.Data.authenticatorEnabled;
            emailField.text = email;
            coachIDFiled.text ="ID : " + coachId.ToString();
            firstLetter.text = email[0].ToString();
        }

        onDone?.Invoke(final);
    }

    public IEnumerator CanUpload(int id, Action<ApiResult<CanUploadResponse>> onDone)
    {
        var body = new CanUploadRequest { id = id };
        yield return PostJson<CanUploadRequest, CanUploadResponse>("/Metaverse/CanUpload", body, withAuth: true, onDone: onDone);
    }

    // =======================
    // Core HTTP
    // =======================

    private IEnumerator PostJson<TReq, TRes>(string path, TReq body, bool withAuth, Action<ApiResult<TRes>> onDone)
    {
        var url = JoinUrl(baseUrl, path);
        var jsonBody = JsonUtility.ToJson(body);

        if (verboseLogs) Debug.Log($"POST {url}\n{jsonBody}");

        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.timeout = Mathf.Max(1, timeoutSeconds);

        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("User-Agent", userAgent);
        if (withAuth && !string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();

        var res = new ApiResult<TRes>
        {
            StatusCode = req.responseCode,
            RawBody = req.downloadHandler?.text ?? "",
            Headers = CollectHeaders(req)
        };

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            res.IsSuccess = false;
            res.ErrorKind = ApiErrorKind.Network;
            res.ErrorMessage = req.error;
        }
        else if (req.result == UnityWebRequest.Result.ProtocolError)
        {
            ClassifyHttpError(req, ref res);
        }
        else if (req.result == UnityWebRequest.Result.DataProcessingError)
        {
            res.IsSuccess = false;
            res.ErrorKind = ApiErrorKind.Other;
            res.ErrorMessage = $"DataProcessingError: {req.error}";
        }
        else
        {
            var parsed = TryParse<TRes>(res.RawBody);
            res.IsSuccess = parsed.ok;
            res.Data = parsed.data;
            res.ErrorKind = parsed.ok ? ApiErrorKind.None : ApiErrorKind.Parse;
            res.ErrorMessage = parsed.ok ? null : parsed.parseError ?? "Failed to parse JSON.";
        }

        if (verboseLogs) Debug.Log($"POST done [{res.StatusCode}] success={res.IsSuccess}\n{res.RawBody}");
        onDone?.Invoke(res);
    }

    private static Dictionary<string, string> CollectHeaders(UnityWebRequest req)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in req.GetResponseHeaders() ?? new Dictionary<string, string>())
            dict[kv.Key] = kv.Value;
        return dict;
    }

    private void ClassifyHttpError<T>(UnityWebRequest req, ref ApiResult<T> res)
    {
        var sc = req.responseCode;
        res.IsSuccess = false;

        if (sc == 401) { res.ErrorKind = ApiErrorKind.Unauthorized; res.ErrorMessage = "Unauthorized (401)."; return; }
        if (sc == 403) { res.ErrorKind = ApiErrorKind.Forbidden; res.ErrorMessage = "Forbidden (403)."; return; }
        if (sc == 429) { res.ErrorKind = ApiErrorKind.RateLimited; res.ErrorMessage = "Rate limited (429)."; return; }
        if (sc >= 500) { res.ErrorKind = ApiErrorKind.Http; res.ErrorMessage = $"Server error ({sc})."; return; }

        res.ErrorKind = ApiErrorKind.Http;
        res.ErrorMessage = $"HTTP {sc}.";
    }

    private static string JoinUrl(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(path)) return baseUrl;
        if (baseUrl.EndsWith("/")) baseUrl = baseUrl.TrimEnd('/');
        if (!path.StartsWith("/")) path = "/" + path;
        return baseUrl + path;
    }

    private (bool ok, T data, string parseError) TryParse<T>(string json)
    {
        try
        {
            var data = JsonUtility.FromJson<T>(json);
            if (data == null) return (false, default, "Null after parse.");
            return (true, data, null);
        }
        catch (Exception ex)
        {
            return (false, default, ex.Message);
        }
    }

    private bool MightBeTypeMismatch(string body)
    {
        if (string.IsNullOrEmpty(body)) return false;
        var l = body.ToLowerInvariant();
        return l.Contains("type") && (l.Contains("string") || l.Contains("number") || l.Contains("int"));
    }





    // Call this from other scripts to fetch the market list for a landId.
    public void FetchNFTMarketList(int landId, Action<ApiResult<NFTMarketListResponse>> callback)
    {
        StartCoroutine(NFTMarketListRoutine(landId, callback));
    }

    private IEnumerator NFTMarketListRoutine(int landId, Action<ApiResult<NFTMarketListResponse>> onDone)
    {
        var body = new NFTMarketListRequest { id = landId };

        // Using withAuth: true since you'll call this after login and many market endpoints are protected.
        // If your server doesn't require auth, flip this to withAuth: false.
        yield return PostJson<NFTMarketListRequest, NFTMarketListResponse>(
            "/nftmarket/list", body, withAuth: true, onDone: r =>
            {
                // Normalize backslashes in URLs returned by the server (e.g., Uploads\\NFTs\\...)
                if (r.IsSuccess && r.Data?.message?.orders != null)
                {
                    foreach (var o in r.Data.message.orders)
                    {
                        if (!string.IsNullOrEmpty(o.imageUrl)) o.imageUrl = o.imageUrl.Replace("\\", "/");
                        if (!string.IsNullOrEmpty(o.animationUrl)) o.animationUrl = o.animationUrl.Replace("\\", "/");
                    }
                }
                onDone?.Invoke(r);
            });
    }



    // ---------- Models (request + response) ----------
    [Serializable] public class GetCodeRequest { public string email; }

    [Serializable] public class LoginRequestNumber { public string email; public int code; }
    [Serializable] public class LoginRequestString { public string email; public string code; }

    [Serializable] public class CanUploadRequest { public int id; } // change to string if API expects "id":"6"

    [Serializable] public class GetCodeResponse { public string message; }

    [Serializable]
    public class LoginResponse
    {
        public string message;
        public string token;
        public int coachId;
        public string status;
        public string walletAddress;
        public bool authenticatorEnabled;
    }

    [Serializable]
    public class CanUploadResponse
    {
        public bool canUpload;     // if API returns this
        public bool success;       // or this
        public string message;
        public int remaining;
        public int limit;
    }



    // --- NFT Market List models ---
    [Serializable] public class NFTMarketListRequest { public int id; }

    [Serializable]
    public class NFTMarketListResponse
    {
        public MarketMessage message;

        [Serializable]
        public class MarketMessage
        {
            public Order[] orders;
            public LandMarket landMarket;
            public OtherLandMarket otherLandMarket;
            // ads / landAds exist in JSON but are omitted here; JsonUtility will ignore them safely.
        }

        [Serializable]
        public class Order
        {
            public int id;
            public int landId;
            public string land;
            public int nftId;
            public string nftType;
            public int model;
            public string modelCaption;
            public int series;
            public string seriesCaption;
            public string expireDate; // ISO-8601 string; parse to DateTime if you need
            public string currency;   // "bnb" or "sixp"
            public double price;
            public double landTaxPercent;
            public double uplineCoachPercent;
            public double masterCoachPercent;
            public int mainSpecification;
            public string mainSpecificationTitle;
            public string mainSpecificationShortTitle;
            public string mainSpecificationUnit;
            public string imageUrl;
            public string animationUrl;
            public int openSeaId;
        }

        [Serializable] public class LandMarket { public int count; public string volume; }
        [Serializable] public class OtherLandMarket { public int count; public string volume; }
    }


    // ---------- Result wrapper ----------
    public enum ApiErrorKind { None, Network, Unauthorized, Forbidden, RateLimited, Http, Parse, Other }

    public class ApiResult<T>
    {
        public bool IsSuccess;
        public long StatusCode;
        public ApiErrorKind ErrorKind;
        public string ErrorMessage;
        public string RawBody;
        public Dictionary<string, string> Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public T Data;

        public override string ToString() =>
            $"[{StatusCode}] {(IsSuccess ? "OK" : ErrorKind.ToString())} :: {ErrorMessage}\nBody: {RawBody}";
    }
}
