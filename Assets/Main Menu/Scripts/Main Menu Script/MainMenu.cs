using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstMissionSceneName = "EggMission";

    public void PlayGame()
{
    UnityEngine.SceneManagement.SceneManager.LoadScene("Loading Screen");
}

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}