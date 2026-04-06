using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonGameOver : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject gameOverPanel;
    public bool pauseTime = true;

    [Header("Scene Names")]
    public string mainMenuSceneName = "Main Menu";
    public string mission2SceneName = "Mission2"; // Mission 2 scene

    [Header("Optional Pause Button")]
    public GameObject pauseButton; // Disable when Game Over

    void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(false); // Disable pause when dead

        if (pauseTime)
            Time.timeScale = 0f; // Freeze game
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mission2SceneName);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}