using UnityEngine;

public class InteractiveBoard : MonoBehaviour
{
    [Header("UI Панель")]
    [SerializeField] private GameObject boardPanel; // Ссылка на панель с изображением

    void OnMouseDown()
    {
        if (boardPanel != null)
        {
            boardPanel.SetActive(true);       // Показываем панель
            Time.timeScale = 0;               // Ставим игру на паузу
        }
    }

    public void CloseBoard()
    {
        if (boardPanel != null)
        {
            boardPanel.SetActive(false);      // Скрываем панель
            Time.timeScale = 1;               // Возвращаем игру
        }
    }
}