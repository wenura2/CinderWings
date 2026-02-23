// BigBossHealth.cs (ADD/UPDATE THESE PARTS)
//
// ✅ Smooth fade in/out boss health UI when player is near
// - Uses CanvasGroup on the healthBarRoot
// - No instant pop, it fades

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BigBossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 300;
    [SerializeField] private int currentHealth;

    [Header("Optional UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Animator (Optional)")]
    public Animator animator;
    public string hurtTrigger = "Hurt";
    public string dieTrigger = "Die";

    [Header("Disable On Death (Optional)")]
    public bool disableCollidersOnDeath = true;
    public bool disableRigidbodyOnDeath = true;

    [Header("Health Bar Visibility (Smooth Fade)")]
    public Transform player;
    public float showDistance = 10f;

    [Tooltip("Drag the parent GameObject that holds the slider/text (ex: BossHealthUI)")]
    public GameObject healthBarRoot;

    [Tooltip("Fade speed (higher = faster)")]
    public float fadeSpeed = 6f;

    [Tooltip("When fading out, hide the object after alpha gets below this")]
    public float hideThreshold = 0.02f;

    private CanvasGroup canvasGroup;
    private bool dead;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();

        if (maxHealth <= 0) maxHealth = 1;
        if (currentHealth <= 0) currentHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Auto-find player by tag if not assigned
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Setup CanvasGroup for smooth fade
        if (healthBarRoot)
        {
            canvasGroup = healthBarRoot.GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = healthBarRoot.AddComponent<CanvasGroup>();

            // start hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            healthBarRoot.SetActive(false);
        }

        UpdateUI();
    }

    void Update()
    {
        HandleHealthBarFade();
    }

    void HandleHealthBarFade()
    {
        if (!healthBarRoot || !canvasGroup || !player) return;

        bool shouldShow = !dead && Vector2.Distance(transform.position, player.position) <= showDistance;

        float targetAlpha = shouldShow ? 1f : 0f;

        // Ensure active while fading in/out
        if (!healthBarRoot.activeSelf)
            healthBarRoot.SetActive(true);

        // Smooth alpha
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Optional: toggle interaction (usually boss bars are non-interactable)
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // When fully faded out, disable the object so it doesn't waste UI updates
        if (!shouldShow && canvasGroup.alpha <= hideThreshold)
        {
            canvasGroup.alpha = 0f;
            healthBarRoot.SetActive(false);
        }
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (animator && !string.IsNullOrWhiteSpace(hurtTrigger))
            animator.SetTrigger(hurtTrigger);

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public bool IsDead() => dead;

    public float GetHealthPercent01()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    void UpdateUI()
    {
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    void Die()
    {
        dead = true;

        if (animator && !string.IsNullOrWhiteSpace(dieTrigger))
            animator.SetTrigger(dieTrigger);

        if (disableCollidersOnDeath)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;
        }

        if (disableRigidbodyOnDeath)
        {
            var r = GetComponent<Rigidbody2D>();
            if (r) r.linearVelocity = Vector2.zero;
        }

        // The fade logic will hide the bar smoothly because dead == true
    }

    [ContextMenu("Debug: Damage 50")]
    void DebugDamage50() => TakeDamage(50);
}