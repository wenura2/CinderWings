using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("Game Over")]
    public GameObject gameOverPanel;

    public bool pauseTime = true;

    [Header("Scene Names")]
    public string mainMenuSceneName = "Main Menu";

    void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // SHOW GAME OVER
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (pauseTime)
            Time.timeScale = 0f;
    }

    // REPLAY MISSION 1
    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // QUIT TO MAIN MENU
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}