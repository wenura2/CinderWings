using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Timing")]
    public float warningDelay = 0.5f;
    public float activeDuration = 0.6f;
    public float resetDelay = 0.8f;

    [Header("Damage")]
    public bool dealFullDamage = true;

    private bool isActive = false;
    private bool isOnCooldown = false;

    private Animator animator;

    private bool playerInside = false;
    private Collider2D currentPlayer;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        currentPlayer = other;

        if (!isActive && !isOnCooldown)
            StartCoroutine(ActivateTrap());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other == currentPlayer)
        {
            playerInside = false;
            currentPlayer = null;
        }
    }

    private IEnumerator ActivateTrap()
    {
        isOnCooldown = true;

        // Warning delay
        yield return new WaitForSeconds(warningDelay);

        // Raise spikes
        isActive = true;
        if (animator) animator.SetBool("Active", true);

        // ✅ Only damage if player is STILL inside
        if (playerInside && currentPlayer != null)
        {
            EggHealth egg = currentPlayer.GetComponent<EggHealth>();
            if (egg != null)
            {
                Vector2 hitDirection = (currentPlayer.transform.position - transform.position).normalized;
                int dmg = dealFullDamage ? egg.maxHits : 1;
                egg.TakeDamage(dmg, hitDirection);
            }
        }

        // Stay active
        yield return new WaitForSeconds(activeDuration);

        // Lower spikes
        if (animator) animator.SetBool("Active", false);
        isActive = false;

        // Cooldown
        yield return new WaitForSeconds(resetDelay);
        isOnCooldown = false;
    }
}