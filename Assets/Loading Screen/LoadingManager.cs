using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleLoadingScreen : MonoBehaviour
{
    public string sceneToLoad = "Mission 1";
    public float minimumLoadTime = 1.5f; // prevents instant flash

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            // When Unity finished loading (progress reaches 0.9)
            if (operation.progress >= 0.9f && timer >= minimumLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}