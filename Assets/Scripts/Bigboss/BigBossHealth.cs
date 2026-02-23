using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BigBossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 300;
    [SerializeField] private int currentHealth;

    [Header("Optional UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Animator (Optional)")]
    public Animator animator;
    public string hurtTrigger = "Hurt";
    public string dieTrigger = "Die";

    [Header("Disable On Death (Optional)")]
    public bool disableCollidersOnDeath = true;
    public bool disableRigidbodyOnDeath = true;

    private bool dead;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();

        if (maxHealth <= 0) maxHealth = 1;

        // If you didn't set currentHealth in inspector, start full
        if (currentHealth <= 0) currentHealth = maxHealth;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    // PlayerController calls this (same pattern as EnemyHealth / BigKnightHealth)
    public void TakeDamage(int amount)
    {
        if (dead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (animator && !string.IsNullOrWhiteSpace(hurtTrigger))
            animator.SetTrigger(hurtTrigger);

        UpdateUI();

        // Let BigBossAI handle stage switching itself (it reads health %)
        // so we do NOT call any AI methods here.

        if (currentHealth <= 0)
            Die();
    }

    public bool IsDead() => dead;

    public int GetCurrentHealth() => currentHealth;

    public float GetHealthPercent01()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    void UpdateUI()
    {
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    void Die()
    {
        dead = true;

        if (animator && !string.IsNullOrWhiteSpace(dieTrigger))
            animator.SetTrigger(dieTrigger);

        if (disableCollidersOnDeath)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;
        }

        if (disableRigidbodyOnDeath)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb) rb.linearVelocity = Vector2.zero;
        }

        // Optional: Destroy after animation
        // Destroy(gameObject, 3f);
    }

    // Debug helper
    [ContextMenu("Debug: Damage 50")]
    void DebugDamage50() => TakeDamage(50);
}