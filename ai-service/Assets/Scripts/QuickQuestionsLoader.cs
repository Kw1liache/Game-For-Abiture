using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuickQuestionsLoader : MonoBehaviour
{
    [Header("UI ссылки")]
    public Transform contentParent;
    public GameObject questionBtnPrefab;
    public InputField questionInput;
    public Button askButton;

    [Header("Файл с вопросами")]
    public TextAsset dialogFile;

    [Header("Настройки")]
    public string currentNpcId = "mascot";

    void Start()
    {
        if (dialogFile != null)
        {
            CreateQuestionButtons();
        }
        else
        {
            Debug.LogError("❌ Файл dialogFile не назначен!");
        }
    }

    void CreateQuestionButtons()
    {
        if (contentParent == null || questionBtnPrefab == null)
        {
            Debug.LogError("❌ Не назначены поля!");
            return;
        }

        // 🔥 ПОЛНАЯ ОЧИСТКА - удаляем ВСЕ дочерние объекты
        Debug.Log($"🗑️ Очищаем Content. Было объектов: {contentParent.childCount}");
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Transform child = contentParent.GetChild(i);
            Debug.Log($"Удаляем: {child.name}");
            Destroy(child.gameObject);
        }

        // Парсим файл
        string[] lines = dialogFile.text.Split('\n');
        bool isTargetNPC = false;
        List<string> foundQuestions = new List<string>();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("NPC:"))
            {
                string npcName = trimmed.Replace("NPC:", "").Trim();
                isTargetNPC = (npcName == currentNpcId);
            }

            if (isTargetNPC && trimmed.StartsWith("ВОПРОС_"))
            {
                int splitIndex = trimmed.IndexOf(':');
                if (splitIndex > -1)
                {
                    string questionText = trimmed.Substring(splitIndex + 1).Trim();
                    foundQuestions.Add(questionText);
                }
            }
        }

        Debug.Log($"✅ Найдено вопросов для '{currentNpcId}': {foundQuestions.Count}");

        // Создаём кнопки
        foreach (string questionText in foundQuestions)
        {
            GameObject btnObj = Instantiate(questionBtnPrefab, contentParent);
            btnObj.name = "QuestionBtn_" + foundQuestions.IndexOf(questionText);

            // 🔥 Ищем текст ВСЕМИ способами
            bool textSet = false;

            // Способ 1: TextMeshProUGUI
            TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = questionText;
                // ✅ ИСПРАВЛЕНО: заменяем устаревший enableWordWrapping на textWrappingMode
                tmpText.textWrappingMode = TextWrappingModes.Normal;
                tmpText.alignment = TextAlignmentOptions.Center;
                textSet = true;
                Debug.Log($"✅ Установлен текст в TextMeshPro: {questionText}");
            }

            // Способ 2: Обычный Text (если TMP не найден)
            if (!textSet)
            {
                Text legacyText = btnObj.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = questionText;
                    legacyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    textSet = true;
                    Debug.Log($"✅ Установлен текст в Legacy Text: {questionText}");
                }
            }

            if (!textSet)
            {
                Debug.LogWarning($"⚠️ Не найден текстовый компонент в кнопке {btnObj.name}!");
            }

            // Настраиваем клик
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string capturedQuestion = questionText; // Захватываем переменную
                btn.onClick.AddListener(() => OnQuestionClicked(capturedQuestion));
            }
        }

        Debug.Log($"📊 Всего создано кнопок: {foundQuestions.Count}");
    }

    void OnQuestionClicked(string questionText)
    {
        Debug.Log("📝 Клик: " + questionText);

        if (questionInput != null)
        {
            questionInput.text = questionText;
        }

        if (askButton != null)
        {
            askButton.onClick.Invoke();
        }
    }
}