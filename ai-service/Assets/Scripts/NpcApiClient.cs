using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class NpcAskRequest
{
    public string npc_id;
    public string question;
    public string player_id;
    public string session_id;
}

[Serializable]
public class SourceItem
{
    public string title;
    public string url;
}

[Serializable]
public class NpcAskResponse
{
    public string npc_id;
    public string answer;
    public List<SourceItem> sources;
}

// Класс для обхода проверки SSL/HTTPS (нужен для HTTP в локальной сети)
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Для разработки в локальной сети всегда возвращаем true
        return true;
    }
}

public class NpcApiClient : MonoBehaviour
{
    [Header("Use n8n webhook URL in production")]
    [SerializeField] private string endpointUrl = "http://10.40.241.72:5678/webhook/urfu-npc-ask";
    [Header("Must match N8N_UNITY_API_KEY in n8n environment")]
    [SerializeField] private string apiKey = "nc4sdf-459hrss-ythnq6";
    [SerializeField] private int timeoutSeconds = 90;

    public IEnumerator AskNpc(
        string npcId,
        string question,
        string playerId,
        string sessionId,
        Action<NpcAskResponse> onSuccess,
        Action<string> onError
    )
    {
        var payload = new NpcAskRequest
        {
            npc_id = npcId,
            question = question,
            player_id = playerId,
            session_id = sessionId
        };

        var json = JsonUtility.ToJson(payload);
        using var request = new UnityWebRequest(endpointUrl, "POST");
        
        // Если используется HTTP (не HTTPS), отключаем проверку сертификата
        if (endpointUrl.StartsWith("http://"))
        {
            request.certificateHandler = new BypassCertificateHandler();
        }
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.SetRequestHeader("X-API-Key", apiKey);
        }
        
        request.timeout = timeoutSeconds;

        yield return request.SendWebRequest();

        // Важно: очищаем certificateHandler, чтобы не было утечек
        request.certificateHandler?.Dispose();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var responseText = request.downloadHandler.text;
            Debug.Log($"Ответ от сервера: {responseText}");
            
            NpcAskResponse response = JsonUtility.FromJson<NpcAskResponse>(responseText);
            onSuccess?.Invoke(response);
            yield break;
        }

        string errorMsg = $"NPC request failed: {request.responseCode} {request.error}";
        Debug.LogError(errorMsg);
        onError?.Invoke(errorMsg);
    }
}
