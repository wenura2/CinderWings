using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BreakableHouse : MonoBehaviour
{
    private bool breaking = false;

    [Header("Hits Required")]
    [SerializeField] private int hitsToBreak = 3; // set in Inspector
    private int currentHits = 0;

    [Header("Hit FX (every hit)")]
    public GameObject dustFxPrefab;   // assign dust particle prefab
    public GameObject fireFxPrefab;   // assign fire particle prefab
    public float hitFxSpreadRadius = 0.3f;
    public float hitFxDuration = 1f;

    [Header("Break FX (final destruction)")]
    public GameObject burnFxPrefab;   // assign big fire prefab
    public GameObject smokeFxPrefab;  // assign smoke prefab
    public int totalFxToSpawn = 6;
    public float spreadRadius = 0.6f;
    public float delayBeforeFade = 0.5f;
    public float fadeDuration = 1.5f;

    [Header("Camera Shake (Optional)")]
    public CinemachineShake cameraShake;
    public float shakeAmplitude = 2f;
    public float shakeDuration = 0.2f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Called when player hits the house
    public void RegisterHit()
    {
        if (breaking) return;

        currentHits++;

        // ✅ Play dust + fire FX on every hit
        PlayHitFX();

        if (currentHits >= hitsToBreak)
        {
            BreakHouse();
        }
    }

    private void PlayHitFX()
    {
        Vector2 offset = Random.insideUnitCircle * hitFxSpreadRadius;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);

        if (dustFxPrefab != null)
        {
            GameObject dust = Instantiate(dustFxPrefab, spawnPos, Quaternion.identity);
            dust.transform.SetParent(transform);
            Destroy(dust, hitFxDuration);
        }

        if (fireFxPrefab != null)
        {
            GameObject fire = Instantiate(fireFxPrefab, spawnPos, Quaternion.identity);
            fire.transform.SetParent(transform);
            Destroy(fire, hitFxDuration);
        }
    }

    private void BreakHouse()
    {
        if (!breaking)
            StartCoroutine(BreakRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        breaking = true;

        if (cameraShake != null)
            cameraShake.Shake(shakeAmplitude, shakeDuration);

        // ✅ Spawn burn + smoke FX for final destruction
        for (int i = 0; i < totalFxToSpawn; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spreadRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);

            if (burnFxPrefab != null)
            {
                GameObject fire = Instantiate(burnFxPrefab, spawnPos, Quaternion.identity);
                fire.transform.SetParent(transform);
                Destroy(fire, fadeDuration);
            }

            if (smokeFxPrefab != null)
            {
                GameObject smoke = Instantiate(smokeFxPrefab, spawnPos, Quaternion.identity);
                smoke.transform.SetParent(transform);
                Destroy(smoke, fadeDuration + 1f);
            }
        }

        yield return new WaitForSeconds(delayBeforeFade);

        // ✅ Fade out house sprite
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // ✅ Notify ObjectiveManager when destroyed
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RegisterBuildingDestroyed();
        }

        Destroy(gameObject);
    }
}
