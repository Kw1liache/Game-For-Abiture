using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI паузы")]
    public GameObject pausePanel;
    
    [Header("Кнопки")]
    public Button resumeButton;
    public Button quitButton;
    public Button girlButton;
    public Button boyButton;
    
    [Header("Ссылки")]
    public Character character;
    
    [Header("Чат NPC (перетащи сюда ChatPanel)")]
    public GameObject chatPanel; 
    
    private bool isPaused = false;
    
    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
    }
    
    void Update()
    {
        // 🚀 ДОБАВЛЕНО: Если нажат Esc, вызываем переключение паузы (даже без PlayerInput)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Если чат открыт — не включаем паузу
            if (chatPanel != null && chatPanel.activeSelf)
            {
                Debug.Log("Чат открыт, пауза не включается");
                return;
            }
            
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    // Этот метод больше НЕ НУЖЕН для работы паузы, но оставим на всякий случай
    public void OnPause(InputValue value)
    {
        // Можно оставить пустым или удалить
    }
    
    void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            Debug.Log("Игра на паузе");
        }
    }
    
    void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            Debug.Log("Игра продолжена");
        }
    }
    
    void ChangeCharacter(int index)
    {
        if (character != null)
        {
            character.ChangeCharacter(index);
            ResumeGame();
        }
    }
    
    void QuitGame()
    {
        Debug.Log("Выход из игры");
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}