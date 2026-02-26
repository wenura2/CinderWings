using UnityEngine;
using System.Collections;

public class EggWinSequenceSimple : MonoBehaviour
{
    [Header("Hook (Call Play() when you win)")]
    public bool testWithKey = true;
    public KeyCode testKey = KeyCode.K;

    [Header("Egg")]
    public Transform eggTransform;                 // drag your Egg object here
    public Animator eggAnimator;                   // optional
    public string hatchTrigger = "Hatch";          // optional

    [Header("Flash + Shake")]
    public ScreenFlashUI screenFlash;              // your existing flash script
    public EggCameraShake cameraShake;             // your existing shake script
    public int flashes = 3;
    public float timeBetweenFlashes = 0.12f;
    public float shakeDuration = 0.18f;
    public float shakeMagnitude = 0.12f;

    [Header("Baby Dragon")]
    public GameObject babyDragonPrefab;
    public float emergeHeight = 0.8f;              // how much it rises from egg
    public float emergeDuration = 1.0f;            // time to rise
    public float goRightDistance = 8f;             // how far to fly right
    public float goRightSpeed = 6f;                // move speed
    public float idleAfterEmerge = 0.2f;           // small pause before flying
    public float vanishFadeDuration = 0.35f;       // fade out before destroy
    public bool flipToRight = false;               // set true if prefab faces left by default

    [Header("Timing")]
    public float hatchToFlashDelay = 0.10f;
    public float spawnDelayAfterFlashes = 0.05f;

    private bool running;

    void Awake()
    {
        if (eggTransform == null && eggAnimator != null)
            eggTransform = eggAnimator.transform;
    }

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

        if (eggTransform == null)
        {
            Debug.LogError("EggWinSequenceSimple: eggTransform is missing.");
            running = false;
            yield break;
        }

        if (babyDragonPrefab == null)
        {
            Debug.LogError("EggWinSequenceSimple: babyDragonPrefab is missing.");
            running = false;
            yield break;
        }

        // 1) Egg hatch animation (optional)
        if (eggAnimator != null && !string.IsNullOrEmpty(hatchTrigger))
            eggAnimator.SetTrigger(hatchTrigger);

        yield return new WaitForSecondsRealtime(hatchToFlashDelay);

        // 2) 3 flashes + shake
        for (int i = 0; i < flashes; i++)
        {
            if (cameraShake != null)
                cameraShake.Shake(shakeDuration, shakeMagnitude);

            if (screenFlash != null)
                yield return StartCoroutine(screenFlash.FlashOnce());

            yield return new WaitForSecondsRealtime(timeBetweenFlashes);
        }

        yield return new WaitForSecondsRealtime(spawnDelayAfterFlashes);

        // 3) Spawn baby dragon at egg position
        Vector3 spawnPos = eggTransform.position;
        spawnPos.z = 0f;

        GameObject dragon = Instantiate(babyDragonPrefab, spawnPos, Quaternion.identity);
        dragon.SetActive(true);

        // Find sprite (for flipping + fade)
        SpriteRenderer sr = dragon.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
        {
            sr.enabled = true;
            sr.flipX = flipToRight;
        }

        // Ensure physics won't interfere
        Rigidbody2D rb = dragon.GetComponent<Rigidbody2D>();
        if (rb == null) rb = dragon.GetComponentInChildren<Rigidbody2D>(true);
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // 4) Slowly come out (rise up)
        Vector3 start = spawnPos;
        Vector3 outPos = spawnPos + Vector3.up * emergeHeight;

        float t = 0f;
        while (t < emergeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / emergeDuration);
            if (dragon != null) dragon.transform.position = Vector3.Lerp(start, outPos, p);
            yield return null;
        }

        // 5) Small pause
        yield return new WaitForSecondsRealtime(idleAfterEmerge);

        // 6) Go right then vanish
        Vector3 rightTarget = outPos + Vector3.right * goRightDistance;

        while (dragon != null && Vector2.Distance(dragon.transform.position, rightTarget) > 0.05f)
        {
            dragon.transform.position = Vector3.MoveTowards(
                dragon.transform.position,
                rightTarget,
                goRightSpeed * Time.unscaledDeltaTime
            );
            yield return null;
        }

        // Fade out then destroy
        if (sr != null && vanishFadeDuration > 0f)
        {
            float ft = 0f;
            Color c = sr.color;
            while (ft < vanishFadeDuration)
            {
                ft += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(1f, 0f, ft / vanishFadeDuration);
                sr.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
        }

        if (dragon != null)
            Destroy(dragon);

        running = false;
    }
}