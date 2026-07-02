using UnityEngine;
using UnityEngine.InputSystem;

public class NPC_Trigger : MonoBehaviour
{
    public GameObject chatPanel;
    public GameObject playerCharacter;
    public GameObject interactPrompt; // ️ ВАЖНО: у каждого NPC должен быть СВОЙ объект!

    private bool isPlayerNear = false;
    private bool isChatOpen = false;
    private Component playerMovementScript;
    private System.Reflection.FieldInfo jumpField;
    private bool jumpCached = false;

    void Start()
    {
        if (chatPanel != null) chatPanel.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Кэшируем поиск только ОДИН раз при старте
        if (playerCharacter != null && !jumpCached)
        {
            var scripts = playerCharacter.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                var field = script.GetType().GetField("canJump") ?? script.GetType().GetField("disableJump");
                if (field != null && field.FieldType == typeof(bool))
                {
                    playerMovementScript = script;
                    jumpField = field;
                    jumpCached = true;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Открытие чата по E
        if (isPlayerNear && !isChatOpen && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChat();
        }

        // Закрытие по ESC
        if (isChatOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseChat();
        }
    }

    void OpenChat()
    {
        if (chatPanel == null) return;
        chatPanel.SetActive(true);
        isChatOpen = true;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        DisablePlayerJump(true);
    }

    void CloseChat()
    {
        if (chatPanel == null) return;
        chatPanel.SetActive(false);
        isChatOpen = false;
        DisablePlayerJump(false);
        if (interactPrompt != null && isPlayerNear) interactPrompt.SetActive(true);
    }

    void DisablePlayerJump(bool disable)
    {
        if (jumpCached && playerMovementScript != null && jumpField != null)
        {
            jumpField.SetValue(playerMovementScript, !disable);
        }
        else if (playerCharacter != null)
        {
            var scripts = playerCharacter.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                string name = script.GetType().Name;
                if (name.Contains("Movement") || name.Contains("Controller") || name.Contains("Player"))
                {
                    script.enabled = !disable;
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (playerCharacter == null) playerCharacter = other.gameObject;
            if (!isChatOpen && interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (isChatOpen) CloseChat();
        }
    }
}