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

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private bool isDead = false;
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead || isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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
        animator.SetBool("isHurt", true);

        // Flash red 3 times
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        animator.SetBool("isHurt", false);

        // Remaining invincibility subtle blink
        float elapsed = 0f;
        float remainingTime = invincibleDuration - 0.6f;
        while (elapsed < remainingTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsed * 8f, 1f);
            spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp(alpha, 0.3f, 1f));
            yield return null;
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");

        animator.SetBool("isDead", true);

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

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        float t = 0;
        Color c = spriteRenderer.color;
        while (t < 1f)
        {
            t += Time.deltaTime;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }

        // Uncomment to reload scene on death:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;
}