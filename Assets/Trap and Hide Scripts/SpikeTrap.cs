using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Timing")]
    public float warningDelay = 0.5f;     // delay before spikes rise
    public float activeDuration = 0.6f;   // how long spikes stay up
    public float resetDelay = 0.8f;       // cooldown before next trigger

    [Header("Damage")]
    public bool dealFullDamage = true;

    private bool isActive = false;
    private bool isOnCooldown = false;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isActive && !isOnCooldown)
        {
            StartCoroutine(ActivateTrap(other));
        }
    }

    private IEnumerator ActivateTrap(Collider2D player)
    {
        isOnCooldown = true;

        // 🔥 Warning delay before spikes come up
        yield return new WaitForSeconds(warningDelay);

        // Activate spikes
        isActive = true;

        if (animator != null)
            animator.SetBool("Active", true);

        // Damage player
        EggHealth egg = player.GetComponent<EggHealth>();
        if (egg != null)
        {
            Vector2 hitDirection = (player.transform.position - transform.position).normalized;

            if (dealFullDamage)
                egg.TakeDamage(egg.maxHits, hitDirection);
            else
                egg.TakeDamage(1, hitDirection);
        }

        // Stay active
        yield return new WaitForSeconds(activeDuration);

        // Lower spikes
        if (animator != null)
            animator.SetBool("Active", false);

        isActive = false;

        // Cooldown
        yield return new WaitForSeconds(resetDelay);

        isOnCooldown = false;
    }
}
