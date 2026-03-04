using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Mission2WinSequenceBoss : MonoBehaviour
{
    [Header("Hook (Call Play() when boss dies)")]
    public bool testWithKey = true;
    public KeyCode testKey = KeyCode.K;

    [Header("Boss (optional reference if you want to fade/disable it here)")]
    public GameObject bossObject;
    public Animator bossAnimator;                  // optional
    public string bossDeathTrigger = "Die";        // optional
    public float bossDeathAnimWait = 0.8f;         // wait before flashes start

    [Header("Flash + Shake")]
    public ScreenFlashUI screenFlash;              // your flash script (must have FlashOnce coroutine)
    public EggCameraShake cameraShake;             // your shake script (Shake(duration, magnitude))
    public int flashes = 6;
    public float timeBetweenFlashes = 0.10f;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.14f;

    [Header("Optional: Explosion / FX")]
    public GameObject explosionPrefab;
    public Transform fxSpawnPoint;                 // if null uses boss position
    public float fxDelay = 0.05f;

    [Header("Timing")]
    public float startFlashesDelay = 0.05f;
    public float extraHoldAfterFlashes = 0.15f;

    [Header("Next Mission (Load Scene)")]
    public bool loadMission3AfterWin = true;
    public string mission3SceneName = "Mission3";
    public float loadDelayAfterSequence = 0.10f;

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

        // Safety: if time was slowed/paused anywhere, we still want the sequence to play
        Time.timeScale = 1f;

        // 1) Boss death animation (optional)
        if (bossAnimator != null && !string.IsNullOrEmpty(bossDeathTrigger))
            bossAnimator.SetTrigger(bossDeathTrigger);

        // 2) Optional FX (explosion)
        if (explosionPrefab != null)
        {
            yield return new WaitForSecondsRealtime(fxDelay);

            Vector3 fxPos;
            if (fxSpawnPoint != null) fxPos = fxSpawnPoint.position;
            else if (bossObject != null) fxPos = bossObject.transform.position;
            else fxPos = Vector3.zero;

            fxPos.z = 0f;
            Instantiate(explosionPrefab, fxPos, Quaternion.identity);
        }

        // Wait for death anim a bit (optional)
        if (bossDeathAnimWait > 0f)
            yield return new WaitForSecondsRealtime(bossDeathAnimWait);

        yield return new WaitForSecondsRealtime(startFlashesDelay);

        // 3) Several flashes + shake
        for (int i = 0; i < flashes; i++)
        {
            if (cameraShake != null)
                cameraShake.Shake(shakeDuration, shakeMagnitude);

            if (screenFlash != null)
                yield return StartCoroutine(screenFlash.FlashOnce());
            else
                yield return null;

            yield return new WaitForSecondsRealtime(timeBetweenFlashes);
        }

        // 4) Optional: hide boss object after flashes
        if (bossObject != null)
            bossObject.SetActive(false);

        yield return new WaitForSecondsRealtime(extraHoldAfterFlashes);

        // 5) Load Mission 3
        if (loadMission3AfterWin && !string.IsNullOrEmpty(mission3SceneName))
        {
            yield return new WaitForSecondsRealtime(loadDelayAfterSequence);
            SceneManager.LoadScene(mission3SceneName);
        }

        running = false;
    }
}