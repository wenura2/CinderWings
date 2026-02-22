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
    [SerializeField] private GameObject dustPrefab; // assign your DustParticles prefab here

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;     // strength of push
    [SerializeField] private float knockbackDuration = 0.2f; // how long before stopping

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private Rigidbody2D rb;

    // Store attacker position for dust direction
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

        lastAttackerPos = attackerPosition; // ✅ save attacker position

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(FlashHurt());

            // Knockback
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

            // Spawn dust animation at enemy position
            if (dustPrefab != null)
            {
                GameObject dust = Instantiate(dustPrefab, transform.position, Quaternion.identity);

                // ✅ Apply force in knockback direction
                Rigidbody2D dustRb = dust.GetComponent<Rigidbody2D>();
                if (dustRb != null)
                {
                    Vector2 knockDir = (transform.position - (Vector3)lastAttackerPos).normalized;

                    // Add a little random spread for natural look
                    Vector2 randomSpread = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
                    dustRb.AddForce((knockDir + randomSpread) * (knockbackForce * 0.5f), ForceMode2D.Impulse);
                }

                // ✅ Destroy dust at the same time as hurt flash
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
            rb.linearVelocity = Vector2.zero; // stop knockback
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

        StartCoroutine(DisappearAfterDelay(1.5f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
