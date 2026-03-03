using UnityEngine;

public class IdleArcher : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRadius = 5f;
    [SerializeField] private int attackDamage = 20;

    private Animator animator;
    private float cooldownTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cooldownTimer = attackCooldown;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= attackRadius)
        {
            // Face player horizontally
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (direction.x < 0)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else
                transform.localScale = new Vector3(1f, 1f, 1f);

            // Attack if cooldown ready
            if (cooldownTimer <= 0f)
            {
                animator.SetTrigger("Attack"); // single parameter
                cooldownTimer = attackCooldown;
            }
        }
    }

    // Called via Animation Event in Archer Attack animation
    public void ShootArrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance > attackRadius) return;

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
