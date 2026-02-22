using System.Collections;
using UnityEngine;

public class BigKnightHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 200;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private BigKnightAI knightAI;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        knightAI = GetComponent<BigKnightAI>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("BigKnight HP: " + currentHealth + "/" + maxHealth);

        StartCoroutine(HurtFlash());

        // Tell AI to play hurt animation
        if (knightAI != null)
            knightAI.OnHurt();

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HurtFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (knightAI != null) knightAI.SetDead();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // Wait for death animation to play
        yield return new WaitForSeconds(1.5f);

        // Fade out
        float t = 0;
        Color c = spriteRenderer.color;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}