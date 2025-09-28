using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LoginPage : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button getCodeButton;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField code;
    [SerializeField] private TextMeshProUGUI errorLine;
    [SerializeField] private GameObject codeSent;
    [SerializeField] protected TextMeshProUGUI codeSentText;
    [SerializeField] protected TextMeshProUGUI getCodeText;
    [SerializeField] private GameObject connectButton;
    [SerializeField] private GameObject infoButton;
    [SerializeField] private GameObject panel;

    [Header("Visuals")]
    [SerializeField] private int codeSentWaitTime = 30;
    [SerializeField] private Color getCodeNormalColor = Color.white;
    [SerializeField] private Color getCodeDisabledColor = Color.gray;

    // Simple, lenient email regex
    private static readonly Regex EmailRegex = new Regex(
        @"^[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );

    void Start()
    {
        // Make the TMP field behave like an email field
        email.contentType = TMP_InputField.ContentType.EmailAddress;

        getCodeButton.onClick.AddListener(TryCode);
        loginButton.onClick.AddListener(TryLogin);

        // Live validation while typing
        email.onValueChanged.AddListener(_ => ValidateUI());
        code.onValueChanged.AddListener(_ => ValidateUI());

        // NEW: when user leaves the email field, show why buttons are disabled
        email.onEndEdit.AddListener(OnEmailEndEdit);

        // (Optional) keep UI responsive when leaving code field too
        code.onEndEdit.AddListener(_ => ValidateUI());

        errorLine.enabled = false;
        codeSent.SetActive(false);
        ValidateUI();
    }

    // ---- Validation helpers ----
    private bool IsValidEmail(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var s = input.Trim();
        return EmailRegex.IsMatch(s);
    }

    private void ValidateUI()
    {
        bool emailOk = IsValidEmail(email.text);
        bool hasCode = !string.IsNullOrWhiteSpace(code.text);

        getCodeButton.interactable = emailOk;
        loginButton.interactable = emailOk && hasCode;

        // Color feedback for Get Code text
        getCodeText.color = getCodeButton.interactable ? getCodeNormalColor : getCodeDisabledColor;

        // Clear error while typing; we'll show it on end-edit if needed
        if (email.isFocused || code.isFocused) errorLine.enabled = false;
    }

    // NEW: explain why buttons are disabled when the user leaves the email field
    private void OnEmailEndEdit(string _)
    {
        bool emailEmpty = string.IsNullOrWhiteSpace(email.text);
        bool emailOk = IsValidEmail(email.text);
        bool hasCode = !string.IsNullOrWhiteSpace(code.text);

        if (emailEmpty)
        {
            errorLine.text = "Email field is required.";
            errorLine.enabled = true;
            return;
        }

        if (!emailOk)
        {
            errorLine.text = "Please enter a valid email address.";
            errorLine.enabled = true;
            return;
        }

        // Email looks good. If Login is disabled, tell them they also need the code.
        if (!hasCode && !loginButton.interactable)
        {
            errorLine.text = "Enter the code to login.";
            errorLine.enabled = true;
        }
        else
        {
            errorLine.enabled = false;
        }
    }

    // ---- Actions ----
    public void TryCode()
    {
        Debug.Log("called");

        if (!IsValidEmail(email.text))
        {
            errorLine.text = string.IsNullOrWhiteSpace(email.text)
                ? "Email field is required for code."
                : "Please enter a valid email address.";
            errorLine.enabled = true;
            return;
        }

        getCodeButton.interactable = false;
        getCodeText.color = getCodeDisabledColor;

        SixpackApiClient.Instance.RequestLoginCodeFor(email.text.Trim(), r =>
        {
            if (!r.IsSuccess)
            {
                getCodeButton.interactable = true;
                getCodeText.color = getCodeNormalColor;
                errorLine.text = "Operation failed. Make sure your email address is valid.";
                errorLine.enabled = true;
                Debug.LogError("Code failed");
            }
            else
            {
                getCodeButton.gameObject.SetActive(false); // if this is your desired flow
                getCodeButton.interactable = true;
                getCodeText.color = getCodeNormalColor;
                errorLine.enabled = false;

                StartCoroutine(CodeSentTimer());
                codeSent.SetActive(true);
                Debug.Log("Code sent");
            }
        });
    }

    public void TryLogin()
    {
        if (!IsValidEmail(email.text))
        {
            errorLine.text = "Please enter a valid email address.";
            errorLine.enabled = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(code.text))
        {
            errorLine.text = "Please enter the code.";
            errorLine.enabled = true;
            return;
        }

        SixpackApiClient.Instance.LoginWithCode(email.text.Trim(), code.text.Trim(), r =>
        {
            if (!r.IsSuccess)
            {
                errorLine.text = "Failed to login, check your credentials.";
                errorLine.enabled = true;
                Debug.LogError("Failed to login");
            }
            else
            {
                errorLine.enabled = false;
                Debug.Log("Welcome! Your token is " + SixpackApiClient.Instance.token);
                connectButton.SetActive(false);
                infoButton.SetActive(true);
                panel.SetActive(false);
            }
        });
    }

    public IEnumerator CodeSentTimer()
    {
        int timer = Mathf.Max(0, codeSentWaitTime);
        while (timer > 0)
        {
            codeSentText.text = $"Code sent [{timer}]";
            yield return new WaitForSeconds(1);
            timer--;
        }
    }

    void OnDisable()
    {
        getCodeButton.onClick.RemoveAllListeners();
        loginButton.onClick.RemoveAllListeners();
        email.onValueChanged.RemoveAllListeners();
        code.onValueChanged.RemoveAllListeners();
        email.onEndEdit.RemoveAllListeners();
        code.onEndEdit.RemoveAllListeners();
    }
}
