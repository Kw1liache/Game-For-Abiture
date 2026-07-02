using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthUIController : MonoBehaviour
{
    public TMP_InputField loginInput;
    public TMP_InputField passwordInput;

    public Button loginButton;
    public Button registerButton;

    public AuthService authService;

    private void Start()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClick);

        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);
    }

    public void OnLoginClick()
    {
        string login = loginInput != null ? loginInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        Debug.Log($"[AUTH_UI] Login clicked. Login={login}, Password={password}");

        if (authService != null)
        {
            StartCoroutine(authService.Login(login, password, loginInput, passwordInput));
        }
        else
        {
            Debug.LogError("[AUTH_UI] AuthService is not assigned!");
        }
    }

    public void OnRegisterClick()
    {
        string login = loginInput != null ? loginInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        Debug.Log($"[AUTH_UI] Register clicked. Login={login}, Password={password}");

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[AUTH_UI] Fields cannot be empty!");
            return;
        }

        if (authService != null)
        {
            StartCoroutine(authService.Register(login, password));
        }
        else
        {
            Debug.LogError("[AUTH_UI] AuthService is not assigned!");
        }
    }
}