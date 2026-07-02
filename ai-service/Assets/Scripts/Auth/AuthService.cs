using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AuthService : MonoBehaviour
{
    public string baseUrl = "http://10.40.241.72";
    public SceneLoader sceneLoader;

    public IEnumerator Login(string login, string password, TMP_InputField loginInput, TMP_InputField passwordInput)
    {
        string url = $"{baseUrl}/api/v1/auth/login";

        var bodyJson = JsonUtility.ToJson(new LoginRequest
        {
            username = login,
            password = password
        });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[AUTH] Login success. Response: {request.downloadHandler.text}");

                if (sceneLoader != null)
                {
                    sceneLoader.LoadMainGame();
                }
                else
                {
                    Debug.LogError("[AUTH] SceneLoader is not assigned!");
                }
            }
            else
            {
                Debug.LogError($"[AUTH] Login error. Result Status: {request.result}");
                Debug.LogError($"[AUTH] HTTP Code: {request.responseCode} | Error: {request.error}");
                Debug.LogError($"[AUTH] Raw Server Response: {request.downloadHandler.text}");
            }
        }
    }

    public IEnumerator Register(string login, string password)
    {
        string url = $"{baseUrl}/api/v1/auth/register";

        var bodyJson = JsonUtility.ToJson(new LoginRequest
        {
            username = login,
            password = password
        });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[AUTH] Register success! Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[AUTH] Register error. HTTP Code: {request.responseCode} | Error: {request.error}");
                Debug.LogError($"[AUTH] Raw Server Response: {request.downloadHandler.text}");
            }
        }
    }

    [System.Serializable]
    private class LoginRequest
    {
        public string username;
        public string password;
    }

    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
