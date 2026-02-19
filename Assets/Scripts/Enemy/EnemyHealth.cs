using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI enemyAI;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Enemy HP: " + currentHealth + "/" + maxHealth);

        // Flash white when hit
        StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HurtFlash()
    {
        spriteRenderer.color = Color.white * 2f; // flash bright
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop enemy AI
        if (enemyAI != null)
            enemyAI.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Debug.Log("Enemy died!");

        // Disable collider so player can walk through
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
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