using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialogController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NpcApiClient apiClient;

    [Header("NPC")]
    [SerializeField] private string npcId = "dean_irit";

    [Header("Session")]
    [SerializeField] private string playerId = "player_1";
    [SerializeField] private string sessionId = "session_1";

    [Header("UI")]
    [SerializeField] private InputField questionInput;
    [SerializeField] private Text answerText;
    [SerializeField] private Button askButton;

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
        if (apiClient == null || questionInput == null || answerText == null)
        {
            Debug.LogError("NpcDialogController is not configured in Inspector.");
            return;
        }

        string question = questionInput.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(question))
        {
            answerText.text = "Введите вопрос для NPC.";
            return;
        }

        askButton.interactable = false;
        answerText.text = "Думаю над ответом...";

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
        var sb = new StringBuilder();
        sb.AppendLine(response.answer);

        if (response.sources != null && response.sources.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Источники:");

            for (int i = 0; i < response.sources.Count; i++)
            {
                var src = response.sources[i];
                sb.Append("- ");
                sb.Append(string.IsNullOrWhiteSpace(src.title) ? "Без названия" : src.title);

                if (!string.IsNullOrWhiteSpace(src.url))
                {
                    sb.Append(" — ");
                    sb.Append(src.url);
                }

                sb.AppendLine();
            }
        }

        answerText.text = sb.ToString();
    }

    private void OnAskError(string error)
    {
        answerText.text = $"Ошибка запроса к NPC: {error}";
    }
}
