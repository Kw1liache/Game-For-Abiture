using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public Button girlButton;
    public Button boyButton;
    public Button resumeButton;
    public GameObject playerCharacter;
    public GameObject pauseMenuPanel;

    private bool isGirlSelected = true;

    void Start()
    {
        Debug.Log("🎮 === CHARACTER SELECTOR START ===");
        Debug.Log("girlButton: " + (girlButton != null ? "OK" : "NULL"));
        Debug.Log("boyButton: " + (boyButton != null ? "OK" : "NULL"));
        Debug.Log("playerCharacter: " + (playerCharacter != null ? "OK" : "NULL"));

        if (girlButton == null)
        {
            Debug.LogError("❌ girlButton НЕ назначен!");
            return;
        }
        if (boyButton == null)
        {
            Debug.LogError("❌ boyButton НЕ назначен!");
            return;
        }

        girlButton.onClick.AddListener(() => {
            Debug.Log("🔴 === КЛИК ПО ДЕВОЧКЕ ===");
            SelectGirl();
        });

        boyButton.onClick.AddListener(() => {
            Debug.Log("🔵 === КЛИК ПО МАЛЬЧИКУ ===");
            SelectBoy();
        });

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => {
                Debug.Log("🟢 === КЛИК ПО ПРОДОЛЖИТЬ ===");
                ResumeGame();
            });
        }

        SelectGirl();
        Debug.Log("🎮 === SELECTOR READY ===");
    }

    void SelectGirl()
    {
        Debug.Log("✅ SelectGirl() вызвана");
        isGirlSelected = true;

        if (playerCharacter == null)
        {
            Debug.LogError("❌ playerCharacter = null");
            return;
        }

        Character characterScript = playerCharacter.GetComponent<Character>();
        if (characterScript == null)
        {
            Debug.LogError("❌ Character script NOT FOUND on Player!");
            return;
        }

        Debug.Log("✅ Вызываем ChangeCharacter(0)");
        characterScript.ChangeCharacter(0);

        UpdateColors();
    }

    void SelectBoy()
    {
        Debug.Log("✅ SelectBoy() вызвана");
        isGirlSelected = false;

        if (playerCharacter == null)
        {
            Debug.LogError("❌ playerCharacter = null");
            return;
        }

        Character characterScript = playerCharacter.GetComponent<Character>();
        if (characterScript == null)
        {
            Debug.LogError("❌ Character script NOT FOUND on Player!");
            return;
        }

        Debug.Log("✅ Вызываем ChangeCharacter(1)");
        characterScript.ChangeCharacter(1);

        UpdateColors();
    }

    void UpdateColors()
    {
        if (girlButton != null)
        {
            ColorBlock c = girlButton.colors;
            c.normalColor = isGirlSelected ? Color.white : new Color(1, 1, 1, 0.5f);
            girlButton.colors = c;
        }

        if (boyButton != null)
        {
            ColorBlock c = boyButton.colors;
            c.normalColor = !isGirlSelected ? Color.white : new Color(1, 1, 1, 0.5f);
            boyButton.colors = c;
        }
    }

    void ResumeGame()
    {
        Debug.Log("✅ ResumeGame() вызвана");
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        else
            gameObject.SetActive(false);

        Time.timeScale = 1f;
    }
}