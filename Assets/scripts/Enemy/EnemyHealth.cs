using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private Color hurtColor = Color.red;

    [Header("Dust Animation")]
    [SerializeField] private GameObject dustPrefab;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1f; // how long to fade out

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private Rigidbody2D rb;

    private Vector2 lastAttackerPos;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(int amount, Vector2 attackerPosition)
    {
        if (isDead) return;

        lastAttackerPos = attackerPosition;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(FlashHurt());

            if (rb != null)
            {
                Vector2 knockDir = (transform.position - (Vector3)attackerPosition).normalized;
                rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                StartCoroutine(StopKnockback());
            }
        }
        else
        {
            Die();
        }
    }

    private IEnumerator FlashHurt()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hurtColor;

            if (dustPrefab != null)
            {
                GameObject dust = Instantiate(dustPrefab, transform.position, Quaternion.identity);
                Rigidbody2D dustRb = dust.GetComponent<Rigidbody2D>();
                if (dustRb != null)
                {
                    Vector2 knockDir = (transform.position - (Vector3)lastAttackerPos).normalized;
                    Vector2 randomSpread = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
                    dustRb.AddForce((knockDir + randomSpread) * (knockbackForce * 0.5f), ForceMode2D.Impulse);
                }
                Destroy(dust, hurtDuration);
            }

            yield return new WaitForSeconds(hurtDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator StopKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Enemy defeated!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        ArcherEnemy archer = GetComponent<ArcherEnemy>();
        if (archer != null) archer.enabled = false;

        // Start fade‑out effect
        StartCoroutine(FadeAndDisappear());
    }

    private IEnumerator FadeAndDisappear()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
