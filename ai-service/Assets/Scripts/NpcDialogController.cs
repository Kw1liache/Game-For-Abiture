using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialogController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NpcApiClient apiClient;

    [Header("NPC")]
    [SerializeField] private string npcId = "mascot";

    [Header("Session")]
    [SerializeField] private string playerId = "player_1";
    [SerializeField] private string sessionId = "session_1";

    [Header("UI")]
    [SerializeField] private InputField questionInput;
    [SerializeField] private TMPro.TMP_InputField answerInputField; 
    [SerializeField] private Button askButton;
    
    // ДОБАВИЛИ: Ссылка на родительский скролл для управления прокруткой
    [SerializeField] private ScrollRect answerScrollRect; 

    private void Awake()
    {
        if (askButton != null)
        {
            askButton.onClick.AddListener(OnAskClicked);
        }
    }

    private void OnDestroy()
    {
        if (askButton != null)
        {
            askButton.onClick.RemoveListener(OnAskClicked);
        }
    }

    public void OnAskClicked()
    {
        if (apiClient == null || questionInput == null || answerInputField == null)
        {
            Debug.LogError("NpcDialogController is not configured in Inspector.");
            return;
        }

        string question = questionInput.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(question))
        {
            answerInputField.text = "Введите вопрос для NPC.";
            return;
        }

        askButton.interactable = false;
        answerInputField.text = "Думаю над ответом...";
        
        // Сбрасываем скролл наверх перед началом ожидания
        ResetScrollToTop();

        StartCoroutine(SendQuestion(question));
    }

    private IEnumerator SendQuestion(string question)
    {
        yield return apiClient.AskNpc(
            npcId,
            question,
            playerId,
            sessionId,
            OnAskSuccess,
            OnAskError
        );

        if (askButton != null)
        {
            askButton.interactable = true;
        }
    }

    private void OnAskSuccess(NpcAskResponse response)
    {
        string cleanText = response.answer.Replace("\\n", "\n");
        
        int sourcesIndex = cleanText.IndexOf("Источники:", System.StringComparison.Ordinal);
        if (sourcesIndex > 0)
        {
            cleanText = cleanText.Substring(0, sourcesIndex).TrimEnd();
        }

        answerInputField.text = cleanText;
        
        // Запускаем корутину сброса, чтобы Unity успела пересчитать размеры UI
        StartCoroutine(ResetScrollCoroutine());
    }

    private void OnAskError(string error)
    {
        answerInputField.text = $"Ошибка запроса к NPC: {error}";
        ResetScrollToTop();
    }

    // Метод для мгновенного сброса
    private void ResetScrollToTop()
    {
        if (answerScrollRect != null)
        {
            answerScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // Корутина, которая ждет 1 кадр (пока обновятся размеры текста) и кидает скролл наверх
    private IEnumerator ResetScrollCoroutine()
    {
        yield return new WaitForEndOfFrame();
        ResetScrollToTop();
    }
}