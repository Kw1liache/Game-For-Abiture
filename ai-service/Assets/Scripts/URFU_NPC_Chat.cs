using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

// Структура запроса к серверу
[System.Serializable]
public class ChatRequest
{
    public string user_message;
    public string npc_name;
    public string npc_personality;
}

// Структура ответа от сервера
[System.Serializable]
public class ChatResponse
{
    public string reply;
}

public class URFU_NPC_Chat : MonoBehaviour
{
    [Header("Настройки NPC-консультанта УрФУ")]
    [Tooltip("Имя персонажа")]
    public string npcName = "Консультант Приёмной комиссии УрФУ";
    
    [Tooltip("Характер и роль NPC (это будет отправляться в нейросеть)")]
    [TextArea(5, 10)]
    public string npcPersonality = @"Ты — консультант Приёмной комиссии Уральского федерального университета (УрФУ).
Твоя задача: помогать абитуриентам с вопросами о поступлении.

Ты знаешь следующую информацию об УрФУ:
- УрФУ находится в Екатеринбурге
- Есть направления: ИТ, инженерия, естественные науки, гуманитарные, экономика
- Сроки подачи документов: с 20 июня по 25 июля (на бюджет)
- Вступительные испытания: ЕГЭ или внутренние экзамены УрФУ
- Есть бюджетные и платные места
- Работает Приёмная комиссия: +7 (343) 375-44-44
- Сайт: urfu.ru

Отвечай дружелюбно, но по делу. Не придумывай несуществующую информацию.
Если не знаешь ответа, скажи, что нужно обратиться в Приёмную комиссию. 
Если требуется отвечай больше чем 1-3 предложения, то пиши в виде списка. Не используй эмодзи в ответах.";

    [Header("🎮 UI элементы (перетащи из иерархии)")]
    public TMP_InputField inputField;        // Поле ввода вопроса
    public TextMeshProUGUI responseText;     // Поле для ответа
    public UnityEngine.UI.Button sendButton; // Кнопка отправки

    [Header("🌐 Настройки сервера")]
    [Tooltip("Адрес твоего FastAPI сервера")]
    public string serverURL = "http://localhost:8000/talk";

    // Анимация "печатания" (опционально)
    private Coroutine thinkingAnimation;

    void Start()
    {
        // Привязываем кнопку
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendMessage);
        
        // Привязываем клавишу Enter в поле ввода
        if (inputField != null)
            inputField.onSubmit.AddListener(delegate { OnSendMessage(); });
        
        // Очищаем поле ответа при старте
        if (responseText != null)
            responseText.text = "Задайте вопрос о поступлении в УрФУ...";
    }

    // Вызывается при нажатии кнопки или Enter
    public void OnSendMessage()
    {
        // Проверяем, что поле не пустое
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            responseText.text = "Напишите ваш вопрос о поступлении в УрФУ.";
            return;
        }
        
        // Сохраняем вопрос и очищаем поле
        string question = inputField.text;
        inputField.text = "";
        
        // Отправляем на сервер
        StartCoroutine(SendToServer(question));
    }

    IEnumerator SendToServer(string question)
    {
        // Показываем анимацию "думает"
        if (thinkingAnimation != null)
            StopCoroutine(thinkingAnimation);
        thinkingAnimation = StartCoroutine(AnimateThinking());
        
        // Блокируем кнопку, чтобы не спамить
        if (sendButton != null)
            sendButton.interactable = false;

        // Формируем JSON запрос
        ChatRequest requestData = new ChatRequest
        {
            user_message = question,
            npc_name = npcName,
            npc_personality = npcPersonality
        };
        
        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Ждём ответ от сервера (таймаут 15 секунд)
            yield return request.SendWebRequest();

            // Останавливаем анимацию
            if (thinkingAnimation != null)
                StopCoroutine(thinkingAnimation);
            
            // Разблокируем кнопку
            if (sendButton != null)
                sendButton.interactable = true;

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Успешно получили ответ
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                responseText.text = $"{npcName}:\n{response.reply}";
            }
            else
            {
                // Ошибка подключения
                responseText.text = $"Ошибка соединения с сервером.\nПроверьте, запущен ли сервер.\nТехническая информация: {request.error}";
                Debug.LogError($"Ошибка подключения: {request.error}");
            }
        }
    }

    // Анимация "Консультант печатает ответ..."
    IEnumerator AnimateThinking()
    {
        string[] messages = {
            "Консультант изучает ваш вопрос...",
            "Анализирую информацию об УрФУ...",
            "Ищу актуальные данные о поступлении...",
            "Формирую ответ..."
        };
        
        int index = 0;
        while (true)
        {
            responseText.text = messages[index % messages.Length];
            index++;
            yield return new WaitForSeconds(0.8f);
        }
    }
}