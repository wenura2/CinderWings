using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth; // visible in Inspector

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.5f; // how long Hurt animation plays

    private Animator animator;
    private PlayerController playerController; // your movement script
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        // Reduce health
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player took damage! Current HP: " + currentHealth);

        if (currentHealth > 0)
        {
            // Play Hurt animation
            animator.SetTrigger("Hurt");
            // Return to Idle/Run after hurtDuration
            StartCoroutine(RecoverFromHurt());
        }
        else
        {
            Die();
        }
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(hurtDuration);
        // Animator transitions will naturally return to Idle/Run if set up
        // Optionally force Idle here if needed:
        // animator.SetTrigger("Idle");
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player defeated!");
        animator.SetTrigger("Death"); // trigger death animation

        // Disable player controls
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Collapse and disappear after delay
        StartCoroutine(DisappearAfterDelay(2f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // hide player completely
    }
}
