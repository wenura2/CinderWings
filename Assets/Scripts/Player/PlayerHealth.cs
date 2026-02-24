using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Hurt Settings")]
    public float hurtDuration = 0.5f;
    public float invincibleDuration = 1f;

    [Header("Events")]
    public System.Action OnDamaged;   // ✅ UI can listen to this (shake bar)
    public System.Action OnDied;      // (optional)

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private bool isDead = false;
    private bool isInvincible = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }
    public void Heal(int amount)
{
    if (isDead) return;
    if (amount <= 0) return;

    currentHealth += amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    Debug.Log("Healed! Player HP: " + currentHealth + "/" + maxHealth);
}

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isInvincible) return;
        if (amount <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnDamaged?.Invoke(); // ✅ fire event for UI shake

        Debug.Log("Player HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
            Die();
        else
        {
            StopAllCoroutines();
            StartCoroutine(HurtRoutine());
        }
    }

    IEnumerator HurtRoutine()
    {
        isInvincible = true;

        if (animator) animator.SetBool("isHurt", true);

        // Flash red 3 times
        if (spriteRenderer)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            // If no sprite renderer, still wait the same time
            yield return new WaitForSeconds(0.6f);
        }

        if (animator) animator.SetBool("isHurt", false);

        // Remaining invincibility subtle blink
        float elapsed = 0f;
        float remainingTime = Mathf.Max(0f, invincibleDuration - 0.6f);

        while (elapsed < remainingTime)
        {
            elapsed += Time.deltaTime;

            if (spriteRenderer)
            {
                float alpha = Mathf.PingPong(elapsed * 8f, 1f);
                spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp(alpha, 0.3f, 1f));
            }

            yield return null;
        }

        if (spriteRenderer) spriteRenderer.color = Color.white;

        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");

        if (animator) animator.SetBool("isDead", true);

        OnDied?.Invoke();

        if (playerController != null)
            playerController.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        StartCoroutine(DeathRoutine());
    }

    public float GetHealth01()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (spriteRenderer)
        {
            float t = 0f;
            Color c = spriteRenderer.color;

            while (t < 1f)
            {
                t += Time.deltaTime;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1f - t);
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;
}