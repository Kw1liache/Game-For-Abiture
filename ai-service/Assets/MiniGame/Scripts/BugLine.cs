using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BugLine : MonoBehaviour
{
    [Header("Настройки строки")]
    public bool isBug = false;
    public string errorMessage = "";

    private Image bgImage;
    private TextMeshProUGUI codeText;
    private bool gameEnded = false;
    private Button button;

    void Start()
    {
        bgImage = GetComponent<Image>();
        codeText = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        bgImage.color = Color.white;
    }

    void OnClick()
    {
        if (gameEnded)
        {
            Debug.Log("⚠️ Кнопка " + gameObject.name + " заблокирована (gameEnded=true)");
            return;
        }

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.CheckAnswer(this);
        }
    }

    public void SetCorrect()
    {
        bgImage.color = Color.green;
        gameEnded = true;
    }

    public void SetWrong()
    {
        bgImage.color = new Color(0.8f, 0.2f, 0.2f);

        if (codeText != null && !string.IsNullOrEmpty(errorMessage))
        {
            codeText.text = errorMessage;
            codeText.color = Color.white;
            codeText.fontSize = 14;
        }
    }

    public void ResetButton()
    {
        Debug.Log("🔄 ResetButton на " + gameObject.name);
        gameEnded = false;
        bgImage.color = Color.white;

        if (codeText != null)
        {
            codeText.color = Color.black;
            codeText.fontSize = 18;
        }
    }

    public void EndGame()
    {
        gameEnded = true;
    }
}