using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private Color hurtColor = Color.red;

    [Header("Dust Animation")]
    [SerializeField] private GameObject dustPrefab;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;      // stronger push
    [SerializeField] private float knockbackDuration = 0.5f;  // longer knockback

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private bool isKnockedBack = false;
    private Rigidbody2D rb;

    private Vector2 lastAttackerPos;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount, Vector2 attackerPosition)
    {
        if (isDead) return;

        lastAttackerPos = attackerPosition;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth > 0)
        {
            // Flash + dust + knockback all together
            StartCoroutine(FlashHurt());

            if (rb != null)
            {
                Vector2 knockDir = (transform.position - (Vector3)attackerPosition).normalized;
                rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                isKnockedBack = true;
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
            // Immediately flash hurt color
            spriteRenderer.color = hurtColor;

            // Immediately spawn dust
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
                Destroy(dust, 0.5f); // quick burst lifetime
            }

            // Wait before resetting color
            yield return new WaitForSeconds(hurtDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator StopKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Enemy defeated!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

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

    public bool IsDead() => isDead;
    public bool IsKnockedBack() => isKnockedBack;
}
