using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.5f;

    [Header("UI Health Bar")]
    public Slider healthBar;          // assign in Inspector
    public Transform healthBarCanvas; // assign the Canvas Transform
    public Vector3 barOffset = new Vector3(0, 2f, 0); // position above dragon
    public Image fillImage;           // assign the Fill image of the Slider

    [Header("Health Bar Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    private Animator animator;
    private DragController playerController;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<DragController>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        UpdateHealthBarColor();
    }

    private void Update()
    {
        // Make health bar follow dragon and face camera
        if (healthBarCanvas != null)
        {
            healthBarCanvas.position = transform.position + barOffset;
            healthBarCanvas.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player took damage! Current HP: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(RecoverFromHurt());
        }
        else
        {
            Die();
        }
    }

    private void UpdateHealthBarColor()
    {
        if (fillImage != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;

            // Blend between colors based on health percentage
            if (healthPercent > 0.5f)
            {
                fillImage.color = Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, healthPercent * 2f);
            }
        }
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(hurtDuration);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player defeated!");
        animator.SetTrigger("Death");

        if (playerController != null)
            playerController.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(DisappearAfterDelay(2f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
}
