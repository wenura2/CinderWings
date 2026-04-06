using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    public GameObject pausePanel;
    public Button pauseButton;
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}