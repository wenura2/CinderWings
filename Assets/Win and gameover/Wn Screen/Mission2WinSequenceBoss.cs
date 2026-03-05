using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission2WinSequenceBoss : MonoBehaviour
{
    [Header("Hook (Call Play() when boss dies)")]
    public bool testWithKey = true;
    public KeyCode testKey = KeyCode.K;

    [Header("Boss (optional reference if you want to fade/disable it here)")]
    public GameObject bossObject;
    public Animator bossAnimator;
    public string bossDeathTrigger = "Die";   // ✅ must match Animator parameter exactly
    public float bossDeathAnimWait = 0.8f;

    [Header("Flash + Shake")]
    public ScreenFlashUI screenFlash;
    public CinemachineShake cameraShake;
    public int flashes = 6;
    public float timeBetweenFlashes = 0.10f;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.14f;

    [Header("Optional: Explosion / FX")]
    public GameObject explosionPrefab;
    public Transform fxSpawnPoint;
    public float fxDelay = 0.05f;

    [Header("Timing")]
    public float startFlashesDelay = 0.05f;

    [Header("Next Mission (Load Scene)")]
    public bool loadMission3AfterWin = true;
    public string mission3SceneName = "Mission3"; // ✅ must match Build Settings scene name

    private bool running;

    void Update()
    {
        if (testWithKey && Input.GetKeyDown(testKey))
            Play();
    }

    public void Play()
    {
        if (running) return;
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        running = true;
        Time.timeScale = 1f;

        // 1) Boss death animation
        if (bossAnimator != null && !string.IsNullOrEmpty(bossDeathTrigger))
            bossAnimator.SetTrigger(bossDeathTrigger);

        // 🔹 Immediately hide all archers when boss dies
        ArcherMove[] archers = Object.FindObjectsByType<ArcherMove>(FindObjectsSortMode.None);
        foreach (ArcherMove archer in archers)
        {
            archer.gameObject.SetActive(false);
        }

        // 2) Explosion FX
        if (explosionPrefab != null)
        {
            yield return new WaitForSecondsRealtime(fxDelay);
            Vector3 fxPos = fxSpawnPoint ? fxSpawnPoint.position :
                           bossObject ? bossObject.transform.position : Vector3.zero;
            fxPos.z = 0f;
            Instantiate(explosionPrefab, fxPos, Quaternion.identity);
        }

        // 3) Wait for death anim
        if (bossDeathAnimWait > 0f)
            yield return new WaitForSecondsRealtime(bossDeathAnimWait);

        yield return new WaitForSecondsRealtime(startFlashesDelay);

        // 4) Flashes + Camera Shake Loop
        for (int i = 0; i < flashes; i++)
        {
            if (cameraShake != null)
                cameraShake.Shake(shakeMagnitude, shakeDuration);

            if (screenFlash != null)
                yield return StartCoroutine(screenFlash.FlashOnce());

            yield return new WaitForSecondsRealtime(timeBetweenFlashes);
        }

        // 5) Hold full white for 2 seconds
        if (screenFlash != null)
            yield return StartCoroutine(screenFlash.HoldWhite(2f));

        // 6) Hide boss
        if (bossObject != null)
            bossObject.SetActive(false);

        // 7) Immediately load Mission 3 (no pause, no reload of Mission2)
        if (loadMission3AfterWin && !string.IsNullOrEmpty(mission3SceneName))
        {
            Debug.Log("Loading scene: " + mission3SceneName);
            SceneManager.LoadScene(mission3SceneName);
        }

        running = false;
    }
}
