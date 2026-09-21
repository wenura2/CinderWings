using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;

    [Header("Game Play / Mission Select")]
    public GameObject gamePlayPanel;

    [Header("Mission 2 Menu")]
    public GameObject mission2MenuPanel;

    [Header("Mission Scenes")]
    public string mission1SceneName = "Mission 1";
    public string mission2SceneName = "Mission 2";

    [Header("Mission Buttons")]
    public GameObject mission2Button;

    private const string Mission2UnlockedKey = "Mission2Unlocked";

    // START BUTTON
    public void StartGame()
    {
        SceneManager.LoadScene(mission1SceneName);
    }

    // GAME PLAY BUTTON
    public void OpenGamePlay()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(true);

        UpdateMissionButtons();
    }

    // BACK TO MAIN MENU
    public void BackToMainMenu()
    {
        if (gamePlayPanel != null)
            gamePlayPanel.SetActive(false);

        if (mission2MenuPanel != null)
            mission2MenuPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // PLAY MISSION 1
    public void PlayMission1()
    {
        SceneManager.LoadScene(mission1SceneName);
    }

    // PLAY MISSION 2
    public void PlayMission2()
    {
        if (IsMission2Unlocked())
        {
            SceneManager.LoadScene(mission2SceneName);
        }
        else
        {
            Debug.Log("Mission 2 is locked. Complete Mission 1 first.");
        }
    }

    // MISSION 1 COMPLETED
    public void CompleteMission1()
    {
        PlayerPrefs.SetInt(Mission2UnlockedKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Mission 1 completed! Mission 2 unlocked.");

        SceneManager.LoadScene(mission2SceneName);
    }

    // CHECK MISSION 2
    public bool IsMission2Unlocked()
    {
        return PlayerPrefs.GetInt(Mission2UnlockedKey, 0) == 1;
    }

    // UPDATE MISSION BUTTON
    private void UpdateMissionButtons()
    {
        if (mission2Button != null)
        {
            mission2Button.SetActive(IsMission2Unlocked());
        }
    }

    // RESET PROGRESS
    public void ResetMissionProgress()
    {
        PlayerPrefs.DeleteKey(Mission2UnlockedKey);
        PlayerPrefs.Save();

        UpdateMissionButtons();

        Debug.Log("Mission progress reset.");
    }

    // QUIT
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}