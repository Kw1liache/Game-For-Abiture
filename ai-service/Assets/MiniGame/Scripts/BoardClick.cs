using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // Обязательно для корутин

public class BoardClick : MonoBehaviour
{
    private bool isLoading = false; 

    void Update()
    {
        if (isLoading) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(LoadMiniGameAsync());
            }
        }
    }

    IEnumerator LoadMiniGameAsync()
    {
        isLoading = true; 

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level1");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isLoading = false; // Снимаем блокировку (на всякий случай)

    }
}