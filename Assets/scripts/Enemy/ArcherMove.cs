using UnityEngine;
using System.Collections.Generic;

public class ArcherMove : MonoBehaviour
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
    private SpriteRenderer sr;
    private Vector3 startPosition;

    // Breadcrumb stack
    private Stack<Vector3> breadcrumbs = new Stack<Vector3>();
    private float breadcrumbInterval = 0.5f;
    private float breadcrumbTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startPosition = transform.position;
        cooldownTimer = attackCooldown;
        animator.ResetTrigger("Attack");
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        bool isMoving = false;

        if (distance <= chaseRadius)
        {
            // Face player (flip sprite only)
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (Mathf.Abs(direction.x) > 0.2f) // slightly bigger threshold to avoid flicker
            {
                sr.flipX = direction.x < 0;
            }

            if (distance <= attackRadius)
            {
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
                isMoving = true;
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
                    isMoving = true;
                else
                    isMoving = false;
            }
        }

        // ✅ Animator parameter matches your controller
        animator.SetBool("isMoving", isMoving);
    }

    // Called via Animation Event in Archer Attack animation
    public void ShootArrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (distance > attackRadius) return;

        Vector2 direction = (player.transform.position - shootPoint.position).normalized;

        // Rotate arrow to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, rotation);

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
