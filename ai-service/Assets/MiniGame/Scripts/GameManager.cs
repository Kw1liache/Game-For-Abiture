using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Подключаем для сцен
using System.Collections;          // Подключаем для корутин

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelTitle;
    public GameObject nextLevelButton;
    public GameObject[] codeButtons;
    public GameObject winScreen;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI winMessage;

    private int score = 0;
    private int currentLevel = 1;
    private bool levelComplete = false;

    void Start()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(false);
        }

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        if (scoreText != null)
        {
            scoreText.text = "Очки: " + score;
        }

        if (currentLevel == 1)
        {
            SetupLevel1();
        }
        else if (currentLevel == 2)
        {
            SetupLevel2();
        }
        else if (currentLevel == 3)
        {
            SetupLevel3();
        }
    }

    public void CheckAnswer(BugLine clickedButton)
    {
        if (levelComplete) return;

        if (!clickedButton.isBug)
        {
            clickedButton.SetCorrect();
            score += 10;

            if (scoreText != null)
            {
                scoreText.text = "Очки: " + score;
            }

            levelComplete = true;

            // Если это 3 уровень - сразу показываем экран победы!
            if (currentLevel == 3)
            {
                Debug.Log("3 уровень пройден! Показываем экран победы");
                Invoke("ShowWinScreen", 1f); // Через 1 секунду
            }
            else
            {
                // Для 1 и 2 уровня показываем кнопку "Следующий уровень"
                if (nextLevelButton != null)
                {
                    nextLevelButton.SetActive(true);
                }
            }
        }
        else
        {
            clickedButton.SetWrong();
            score -= 5;
            if (score < 0) score = 0;

            if (scoreText != null)
            {
                scoreText.text = "Очки: " + score;
            }
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        levelComplete = false;

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(false);
        }

        if (currentLevel == 2)
        {
            SetupLevel2();
        }
        else if (currentLevel == 3)
        {
            SetupLevel3();
        }
    }

    // ✅ ПОЛНОСТЬЮ ПЕРЕПИСАННЫЙ ExitGame (АСИНХРОННЫЙ)
    public void ExitGame()
    {
        Debug.Log("ExitGame вызван! Возвращаемся на главную сцену");
        StartCoroutine(LoadMainSceneAsync());
    }

    IEnumerator LoadMainSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("✅ Главная сцена загружена!");
    }

    void ShowWinScreen()
    {
        Debug.Log("ShowWinScreen вызван! Финальный счёт: " + score);

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Твои очки: " + score;
        }

        if (winMessage != null)
        {
            if (score >= 30)
            {
                winMessage.text = "Ты крутой специалист!";
            }
            else if (score >= 20)
            {
                winMessage.text = "Хороший результат!";
            }
            else
            {
                winMessage.text = "У тебя ещё всё получится!";
            }
        }

        foreach (var btn in codeButtons)
        {
            if (btn != null) btn.SetActive(false);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(false);
        }
    }

    void SetupLevel1()
    {
        if (levelTitle != null)
        {
            levelTitle.text = "УРОВЕНЬ 1: PYTHON BASICS";
        }

        if (codeButtons.Length >= 5)
        {
            SetButtonText(0, "Print(\"Hello\")", true, "Ошибка: большая буква P");
            SetButtonText(1, "int x = 5", true, "Ошибка: не Python синтаксис");
            SetButtonText(2, "print(\"Hello\")", false, "");
            SetButtonText(3, "if x > 5", true, "Ошибка: нет двоеточия");
            SetButtonText(4, "print \"Hello\"", true, "Ошибка: нет скобок");
        }

        ResetButtonsColor();
    }

    void SetupLevel2()
    {
        if (levelTitle != null)
        {
            levelTitle.text = "УРОВЕНЬ 2: ПЕРЕМЕННЫЕ";
        }

        if (codeButtons.Length >= 5)
        {
            SetButtonText(0, "int x = 10", true, "Ошибка: не Python синтаксис");
            SetButtonText(1, "x == 10", true, "Ошибка: это сравнение");
            SetButtonText(2, "x = 10", false, "");
            SetButtonText(3, "10 = x", true, "Ошибка: нельзя присваивать числу");
            SetButtonText(4, "var x = 10", true, "Ошибка: нет var в Python");
        }

        ResetButtonsColor();
    }

    void SetupLevel3()
    {
        if (levelTitle != null)
        {
            levelTitle.text = "УРОВЕНЬ 3: СПИСКИ";
        }

        if (codeButtons.Length >= 5)
        {
            SetButtonText(0, "my_list = (1, 2, 3)", true, "Ошибка: это кортеж");
            SetButtonText(1, "my_list = {1, 2, 3}", true, "Ошибка: это множество");
            SetButtonText(2, "my_list = [1, 2, 3]", false, "");
            SetButtonText(3, "list my_list = [1,2,3]", true, "Ошибка: не Python синтаксис");
            SetButtonText(4, "my_list = [1 2 3]", true, "Ошибка: нет запятых");
        }

        ResetButtonsColor();
    }

    void SetButtonText(int index, string text, bool isBug, string error)
    {
        if (index < codeButtons.Length && codeButtons[index] != null)
        {
            TextMeshProUGUI txt = codeButtons[index].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = text;

            BugLine bl = codeButtons[index].GetComponent<BugLine>();
            if (bl != null)
            {
                bl.isBug = isBug;
                bl.errorMessage = error;
            }
        }
    }

    void ResetButtonsColor()
    {
        foreach (var btn in codeButtons)
        {
            if (btn != null)
            {
                btn.SetActive(true);

                BugLine bugLine = btn.GetComponent<BugLine>();
                if (bugLine != null)
                {
                    bugLine.ResetButton();
                }

                Image img = btn.GetComponent<Image>();
                if (img != null) img.color = Color.white;

                TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.color = Color.black;
                    txt.fontSize = 18;
                }
            }
        }
    }
}