using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainGame()
    {
        StartCoroutine(LoadSceneAsync("SampleScene"));
    }

    public void LoadAuthScene()
    {
        StartCoroutine(LoadSceneAsync("AuthScene"));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log($"✅ Сцена {sceneName} загружена!");
    }
}