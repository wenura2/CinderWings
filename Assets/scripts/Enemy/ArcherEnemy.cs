using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRadius = 5f;
    [SerializeField] private int attackDamage = 20;

    private float cooldownTimer;
    private Animator animator;
    private SpriteRenderer sr;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        cooldownTimer = attackCooldown;
        animator.ResetTrigger("Attack"); // prevent accidental trigger at start
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);

        // ✅ Face player by flipping sprite
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (Mathf.Abs(direction.x) > 0.1f) // avoid flicker when player is directly above/below
        {
            sr.flipX = direction.x < 0;
        }

        // Attack if player is within radius
        if (distance <= attackRadius && cooldownTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            cooldownTimer = attackCooldown;
        }
    }

    // Called via Animation Event in Archer Attack animation
    public void ShootArrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance > attackRadius) return; // safeguard

        Vector2 direction = (player.transform.position - shootPoint.position).normalized;
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);

        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
