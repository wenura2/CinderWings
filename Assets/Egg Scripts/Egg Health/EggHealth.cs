using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EggHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHits = 4;
    [SerializeField] private int currentHits = 0;

    [Header("Invincibility")]
    public float invincibleTime = 0.6f;
    private bool isInvincible = false;

    [Header("Stealth")]
    public bool isHidden = false;

    [Header("Damage Feedback")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("Camera Shake")]
    public EggCameraShake cameraShake;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    [Header("Knockback")]
    public float knockbackForce = 2f;

    [Header("Hit Freeze")]
    public float hitFreezeDuration = 0.12f;

    [Header("Death")]
    public float destroyDelay = 1f; // use this as "die animation length"

    [Header("Game Over")]
    public GameOverController gameOver;   // ✅ drag your GameOverController here
    public bool pauseOnGameOver = true;

    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Animator animator;
    private EggMovement eggMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        eggMovement = GetComponent<EggMovement>();

        if (spriteRenderer)
            originalColor = spriteRenderer.color;
    }

    // =========================
    // DAMAGE
    // =========================
    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (animator)
            animator.SetBool("FullyHealed", false);

        if (isDead || isInvincible)
            return;

        currentHits += amount;
        currentHits = Mathf.Clamp(currentHits, 0, maxHits);

        UpdateFractureVisual();

        if (spriteRenderer)
            StartCoroutine(DamageFlash());

        if (cameraShake != null)
            cameraShake.Shake(shakeDuration, shakeMagnitude);

        if (rb && hitDirection != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        if (hitFreezeDuration > 0)
            StartCoroutine(HitFreeze());

        if (currentHits >= maxHits)
            Die();
        else
            StartCoroutine(InvincibilityRoutine());
    }

    // =========================
    // HEAL
    // =========================
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHits -= amount;
        currentHits = Mathf.Clamp(currentHits, 0, maxHits);

        UpdateFractureVisual();
    }

    public bool IsFullyHealed() => currentHits <= 0;

    // =========================
    // VISUAL UPDATE
    // =========================
    private void UpdateFractureVisual()
    {
        if (animator)
            animator.SetInteger("FractureLevel", currentHits);
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

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private IEnumerator HitFreeze()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(hitFreezeDuration);

        Time.timeScale = originalTimeScale;
    }

    // =========================
    // DEATH + GAME OVER
    // =========================
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator)
            animator.SetTrigger("Death");

        if (rb)
            rb.velocity = Vector2.zero;

        if (eggMovement)
            eggMovement.Die(); // stops movement

        // show game over after animation time
        StartCoroutine(GameOverAfterDelay());
    }

    private IEnumerator GameOverAfterDelay()
    {
        // wait for your death animation to finish
        yield return new WaitForSecondsRealtime(destroyDelay);

        if (gameOver != null)
        {
            gameOver.pauseTime = pauseOnGameOver;
            gameOver.ShowGameOver();
        }
        else
        {
            Debug.LogError("EggHealth: GameOverController not assigned!");
        }

        // ✅ DO NOT destroy the player here (UI needs scene alive)
        // Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 25), $"Egg Hits: {currentHits}/{maxHits}");
        GUI.Label(new Rect(10, 30, 200, 25), $"Hidden: {isHidden}");
        GUI.Label(new Rect(10, 50, 200, 25), $"Invincible: {isInvincible}");
    }
#endif
}