using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BreakableCrystal : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool breaking = false;

    [Header("Pulse (Always On)")]
    public float pulseSpeed = 6f;
    [Range(0f, 1f)] public float pulseStrength = 0.4f;

    [Header("Pulse Colors by Health")]
    [Range(0f, 1f)] public float lowHealthPercent = 0.4f;
    [Range(0f, 1f)] public float criticalHealthPercent = 0.2f;
    public Color normalPulseColor = Color.cyan;
    public Color lowHealthPulseColor = Color.yellow;
    public Color criticalPulseColor = Color.red;

    [Header("Hit FX (plays every hit)")]
    public GameObject hitFxPrefab;
    public float hitFxSpreadRadius = 0.1f;
    public float hitFxCooldown = 0.05f; // prevents spam if multiple collisions in a frame

    [Header("Break FX Prefabs (Add 3–4 Here)")]
    public GameObject[] breakFxPrefabs;

    [Header("Break FX Settings")]
    public int totalFxToSpawn = 6;
    public float spreadRadius = 0.4f;
    public float delayBeforeFade = 0.25f;

    [Header("Fade Out")]
    public float fadeDuration = 0.5f;

    [Header("Camera Shake (Cinemachine 3)")]
    public CinemachineShake cameraShake;
    public float shakeAmplitude = 2f;
    public float shakeDuration = 0.15f;

    [Header("Objective")]
    public bool countForObjective = true;

    private SpriteRenderer sr;
    private float nextHitFxTime = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (breaking) return;

        float hp = (float)currentHealth / maxHealth;

        Color baseColor =
            (hp <= criticalHealthPercent) ? criticalPulseColor :
            (hp <= lowHealthPercent) ? lowHealthPulseColor :
            normalPulseColor;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float brightness = Mathf.Lerp(1f - pulseStrength, 1f, t);

        sr.color = baseColor * brightness;
    }

    public void TakeDamage(int damage)
    {
        if (breaking) return;

        // ✅ hit explosion FX
        PlayHitFX();

        currentHealth -= damage;

        if (currentHealth <= 0)
            StartCoroutine(BreakRoutine());
    }

    private void PlayHitFX()
    {
        if (hitFxPrefab == null) return;
        if (Time.time < nextHitFxTime) return;

        nextHitFxTime = Time.time + hitFxCooldown;

        Vector2 offset = Random.insideUnitCircle * hitFxSpreadRadius;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);

        Instantiate(hitFxPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator BreakRoutine()
    {
        breaking = true;

        // ✅ shake camera on break
        if (cameraShake != null)
            cameraShake.Shake(shakeAmplitude, shakeDuration);

        // ✅ Spawn break FX around crystal
        if (breakFxPrefabs != null && breakFxPrefabs.Length > 0)
        {
            for (int i = 0; i < totalFxToSpawn; i++)
            {
                GameObject fx = breakFxPrefabs[Random.Range(0, breakFxPrefabs.Length)];
                if (fx == null) continue;

                Vector2 offset = Random.insideUnitCircle * spreadRadius;
                Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);
                Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

                Instantiate(fx, spawnPos, rot);
            }
        }

        // Count objective once
        if (countForObjective && CrystalObjectiveManager.Instance != null)
            CrystalObjectiveManager.Instance.RegisterCrystalBroken();

        yield return new WaitForSeconds(delayBeforeFade);

        // Fade out crystal
        float elapsed = 0f;
        Color start = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}