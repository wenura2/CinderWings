using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;   // Drag Arrow prefab here
    [SerializeField] private Transform shootPoint;     // Empty GameObject at bow tip
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float detectionRadius = 5f; // Player must be within this radius
    [SerializeField] private int attackDamage = 20;    // Damage per arrow, editable in Inspector

    private float cooldownTimer = 0f;
    private Animator animator;
    private Vector3 originalScale; // store starting scale

    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale; // remember prefab’s original scale
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Find the Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Check distance
        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= detectionRadius)
        {
            // Face the Player without shrinking
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (direction.x < 0)
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            else
                transform.localScale = originalScale;

            // Attack if cooldown ready
            if (cooldownTimer <= 0f)
            {
                animator.SetTrigger("Attack"); // Plays Shooter animation
                cooldownTimer = attackCooldown;
            }
        }
    }

    // Called via Animation Event in Archer Attack animation
    public void ShootArrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Direction toward Player
        Vector2 direction = (player.transform.position - shootPoint.position).normalized;

        // Spawn arrow prefab
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);

        // Send direction + damage to projectile script
        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetDamage(attackDamage); // pass damage value
        }
    }

    // Optional: Draw detection radius in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
