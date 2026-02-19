using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;   // Drag Arrow prefab here
    [SerializeField] private Transform shootPoint;     // Empty GameObject at bow tip
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float detectionRadius = 5f; // How close the player must be

    private float cooldownTimer = 0f;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Find the player (dragon)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Check distance
        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= detectionRadius)
        {
            // Face the player
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1); // face left
            else
                transform.localScale = new Vector3(1, 1, 1);  // face right

            // Attack if cooldown ready
            if (cooldownTimer <= 0f)
            {
                animator.SetTrigger("Attack"); // Plays Shooter animation
                cooldownTimer = attackCooldown;
            }
        }
    }

    // Called via Animation Event in Archer-Shooter-Black
    public void ShootArrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Direction toward player
        Vector2 direction = (player.transform.position - shootPoint.position).normalized;

        // Spawn arrow prefab
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);

        // Send direction to projectile script
        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }
    }
}
