using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Hurt Settings")]
    public float hurtDuration = 0.5f;
    public float invincibleDuration = 1f;   // brief invincibility after getting hit

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
        // Ignore damage if dead or invincible
        if (isDead || isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    IEnumerator HurtRoutine()
    {
        isInvincible = true;

        // Play hurt animation
        animator.SetBool("isHurt", true);

        // Flash the sprite red
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hurtDuration);
        spriteRenderer.color = Color.white;

        animator.SetBool("isHurt", false);

        // Brief invincibility after hurt so enemy can't spam damage
        yield return new WaitForSeconds(invincibleDuration - hurtDuration);
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");

        animator.SetBool("isDead", true);

        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // Disable physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // After death animation finishes, you can reload scene or show game over
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        // Fade out sprite
        float t = 0;
        Color c = spriteRenderer.color;
        while (t < 1f)
        {
            t += Time.deltaTime;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }

        // OPTION A: Reload scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // OPTION B: Just disable
        gameObject.SetActive(false);
    }

    public bool IsDead() => isDead;
}