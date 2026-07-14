using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneNavigator : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Scene Lobby";
    [SerializeField] private string gameSceneName = "Scene Stage";

    public void LoadStartScene()
    {
        LoadScene(startSceneName);
    }

    public void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }

    private static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is not assigned.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
