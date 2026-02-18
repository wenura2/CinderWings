using UnityEngine;
using System.Collections;

public class EggHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHits = 4;
    private int currentHits = 0;

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

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount)
    {
        currentHits += amount;
        currentHits = Mathf.Clamp(currentHits, 0, maxHits);

        // Update Animator fracture level
        animator.SetInteger("FractureLevel", currentHits);

        // Feedback
        StartCoroutine(DamageFlash());
        if (cameraShake != null)
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

        // Death check
        if (currentHits >= maxHits)
        {
            OnEggDestroyed();
        }
    }

    private void DestroyEgg()
{
    Destroy(gameObject);
}


    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = flashColor;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }

    private void OnEggDestroyed()
    {
        Debug.Log("Egg destroyed!");
        animator.SetTrigger("Death");
    }
}
