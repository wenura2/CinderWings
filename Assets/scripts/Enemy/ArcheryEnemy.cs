using UnityEngine;
using System.Collections.Generic;

public class ArcherEnemy : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRadius = 5f;
    [SerializeField] private float chaseRadius = 8f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float moveSpeed = 3f;

    private float cooldownTimer;
    private Animator animator;
    private Vector3 originalScale;
    private Vector3 startPosition;

    // Breadcrumb stack
    private Stack<Vector3> breadcrumbs = new Stack<Vector3>();
    private float breadcrumbInterval = 0.5f; // seconds between drops
    private float breadcrumbTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        startPosition = transform.position;
        cooldownTimer = attackCooldown;

        // ✅ Prevent accidental attack trigger at start
        animator.ResetTrigger("Attack");
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        bool isMoving = false; // track movement for Run animation

        if (distance <= chaseRadius)
        {
            // Face player
            Vector2 direction = (player.transform.position - transform.position).normalized;
            transform.localScale = direction.x < 0
                ? new Vector3(-originalScale.x, originalScale.y, originalScale.z)
                : originalScale;

            if (distance <= attackRadius)
            {
                // Attack
                if (cooldownTimer <= 0f)
                {
                    animator.SetTrigger("Attack");
                    cooldownTimer = attackCooldown;
                }
            }
            else
            {
                // Chase player
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.transform.position,
                    moveSpeed * Time.deltaTime
                );
                isMoving = true;

                // Drop breadcrumbs while chasing
                breadcrumbTimer -= Time.deltaTime;
                if (breadcrumbTimer <= 0f)
                {
                    breadcrumbs.Push(transform.position);
                    breadcrumbTimer = breadcrumbInterval;
                }
            }
        }
        else
        {
            // Return to start using breadcrumbs
            if (breadcrumbs.Count > 0)
            {
                Vector3 targetPos = breadcrumbs.Pop();
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                isMoving = true; // ✅ Run while retracing breadcrumbs
            }
            else
            {
                // If no breadcrumbs left, go straight to start position
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    startPosition,
                    moveSpeed * Time.deltaTime
                );

                if (Vector2.Distance(transform.position, startPosition) > 0.05f)
                    isMoving = true; // ✅ Run until fully back
                else
                    isMoving = false; // Idle once exactly at start
            }
        }

        // ✅ Update Animator for Run/Idle
        animator.SetBool("isRunning", isMoving);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
