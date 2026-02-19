using UnityEngine;
using System.Collections;

public class EggHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHits = 4;
    [SerializeField] private int currentHits = 0; // Visible in Inspector

    [Header("Invincibility")]
    public float invincibleTime = 0.6f;
    private bool isInvincible = false;

    [Header("Stealth")]
    public bool isHidden = false;

    [Header("Damage Feedback")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    [Header("Animation")]
    public Animator animator;

    [Header("Knockback")]
    public Rigidbody2D rb;
    public float knockbackForce = 2f;

    [Header("Hit Freeze")]
    public float hitFreezeDuration = 0.12f;

    [Header("Death")]
    public float destroyDelay = 1f;

    private bool isDead = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;

        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        Debug.Log("Egg TakeDamage called");

        if (isDead)
        {
            Debug.Log("Egg already dead");
            return;
        }

        if (isInvincible)
        {
            Debug.Log("Egg is invincible - damage ignored");
            return;
        }

        currentHits += amount;
        currentHits = Mathf.Clamp(currentHits, 0, maxHits);

        Debug.Log("Egg Hits: " + currentHits + " / " + maxHits);

        if (animator)
            animator.SetInteger("FractureLevel", currentHits);

        if (spriteRenderer)
            StartCoroutine(DamageFlash());

        if (cameraShake != null)
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

        if (rb != null && hitDirection != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        if (hitFreezeDuration > 0f)
            StartCoroutine(HitFreeze());

        if (currentHits >= maxHits)
            Die();
        else
            StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator HitFreeze()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(hitFreezeDuration);

        Time.timeScale = originalTimeScale;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            spriteRenderer.color = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Egg destroyed!");

        if (animator)
            animator.SetTrigger("Death");

        if (rb)
            rb.velocity = Vector2.zero;

        StartCoroutine(DestroyAfterDelay(destroyDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        // Debug UI on screen
        GUI.Label(new Rect(10, 10, 200, 30), "Egg Hits: " + currentHits + "/" + maxHits);
        GUI.Label(new Rect(10, 30, 200, 30), "Hidden: " + isHidden);
        GUI.Label(new Rect(10, 50, 200, 30), "Invincible: " + isInvincible);
    }
#endif
}
