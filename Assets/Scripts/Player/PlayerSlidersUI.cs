using UnityEngine;
using UnityEngine.UI;

public class PlayerSlidersUI : MonoBehaviour
{
    [Header("Auto Find (no dragging needed)")]
    public PlayerHealth playerHealth;
    public PlayerController playerController;

    [Header("Sliders")]
    public Slider healthSlider;
    public Slider fireSlider;

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    [Header("Health Shake")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 6f;

    private RectTransform healthRect;
    private Vector2 healthOriginalAnchoredPos;
    private float shakeTimer;

    void Awake()
    {
        // Auto-find player scripts
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerController = FindFirstObjectByType<PlayerController>();

        // Setup sliders
        SetupSlider(healthSlider);
        SetupSlider(fireSlider);

        // Cache rect for shake
        if (healthSlider != null)
        {
            healthRect = healthSlider.GetComponent<RectTransform>();
            healthOriginalAnchoredPos = healthRect.anchoredPosition;
        }
    }

    void OnEnable()
    {
        // Subscribe to damage event (shake)
        if (playerHealth != null)
            playerHealth.OnDamaged += TriggerHealthShake;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDamaged -= TriggerHealthShake;
    }

    void SetupSlider(Slider s)
    {
        if (!s) return;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.interactable = false;
    }

    void TriggerHealthShake()
    {
        shakeTimer = shakeDuration;
    }

    void Update()
    {
        // Re-find if player respawned / disabled
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();

        // Health slider fill
        if (playerHealth != null && healthSlider != null)
        {
            float target = Mathf.Clamp01(playerHealth.GetHealth01());
            healthSlider.value = Mathf.Lerp(healthSlider.value, target, Time.deltaTime * smoothSpeed);
        }

        // Fire slider fill
        if (playerController != null && fireSlider != null)
        {
            float target = Mathf.Clamp01(playerController.GetFireBreathMeter01());
            fireSlider.value = Mathf.Lerp(fireSlider.value, target, Time.deltaTime * smoothSpeed);
        }

        // Shake health bar UI
        if (healthRect != null)
        {
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector2 offset = Random.insideUnitCircle * shakeStrength;
                healthRect.anchoredPosition = healthOriginalAnchoredPos + offset;

                if (shakeTimer <= 0f)
                    healthRect.anchoredPosition = healthOriginalAnchoredPos;
            }
        }
    }
}