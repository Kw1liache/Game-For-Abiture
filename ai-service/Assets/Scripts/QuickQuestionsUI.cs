using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuickQuestionsUI : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [Tooltip("Перетащи сюда объект Content из QuestionsScroll")]
    public Transform contentParent;

    [Tooltip("Префаб кнопки вопроса из папки Assets")]
    public GameObject questionBtnPrefab;

    [Header("Связь с существующим чатом")]
    [Tooltip("Поле ввода вопроса (InputField)")]
    public InputField inputField;

    [Tooltip("Кнопка 'Спросить'")]
    public Button askButton;

    [Header("Список вопросов")]
    public List<string> questions = new List<string>
    {
        "Чем знаменит ИРИТ-РТФ?",
        "Где найти расписание и документы?",
        "Как попасть в КВН или спорт?",
        "Какие предметы ЕГЭ нужны для поступления?"
    };

    void Start()
    {
        GenerateButtons();
    }

    void GenerateButtons()
    {
        if (contentParent == null || questionBtnPrefab == null) return;

        foreach (string q in questions)
        {
            GameObject btnObj = Instantiate(questionBtnPrefab, contentParent);

            // Записываем текст вопроса в кнопку
            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = q;

            // Привязываем клик
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnQuickQuestionClick(q));
            }
        }
    }

    // 🔥 ТОТ САМЫЙ МЕХАНИЗМ: подставляем текст и жмём "Спросить"
    void OnQuickQuestionClick(string question)
    {
        if (inputField != null)
        {
            inputField.text = question; // Визуально заполняем поле
        }

        if (askButton != null)
        {
            askButton.onClick.Invoke(); // Программно нажимаем твою кнопку "Спросить"
        }
    }
}