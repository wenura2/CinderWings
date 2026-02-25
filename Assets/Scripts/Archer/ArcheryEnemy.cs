using UnityEngine;
using System.Collections;

public class ArcherEnemy : MonoBehaviour
{
    private enum ArcherState { PatrolUp, ReturnUp, Idle, PatrolDown, ReturnDown, Attack }
    private ArcherState currentState = ArcherState.Idle;

    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRadius = 5f;
    [SerializeField] private int attackDamage = 20;

    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDuration = 2f;   // move time (2s)
    [SerializeField] private float idleDuration = 2f;   // idle time (2s)
    [SerializeField] private float patrolDistance = 3f; // distance up/down

    private Animator animator;
    private Vector3 originalScale;
    private Vector3 startPosition;
    private float cooldownTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        startPosition = transform.position;
        cooldownTimer = attackCooldown;
        StartCoroutine(StateMachineRoutine());
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= attackRadius)
        {
            // ✅ Switch to Attack state immediately
            currentState = ArcherState.Attack;

            // Face player horizontally (flip only on X)
            Vector2 direction = (player.transform.position - transform.position).normalized;
            transform.localScale = direction.x < 0
                ? new Vector3(-originalScale.x, originalScale.y, originalScale.z)
                : originalScale;

            // Fire continuously on cooldown
            if (cooldownTimer <= 0f)
            {
                animator.SetTrigger("Attack"); // plays Archer_Shoot_Black
                cooldownTimer = attackCooldown;
            }
        }
        else
        {
            // ✅ Resume patrol cycle if player leaves
            if (currentState == ArcherState.Attack)
                currentState = ArcherState.Idle;
        }
    }

    private IEnumerator StateMachineRoutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case ArcherState.PatrolUp:
                    animator.SetBool("isRunning", true);
                    yield return MoveTo(startPosition + new Vector3(0f, patrolDistance, 0f), moveDuration);
                    currentState = ArcherState.ReturnUp;
                    break;

                case ArcherState.ReturnUp:
                    animator.SetBool("isRunning", true);
                    yield return MoveTo(startPosition, moveDuration);
                    currentState = ArcherState.Idle;
                    break;

                case ArcherState.Idle:
                    animator.SetBool("isRunning", false);
                    yield return new WaitForSeconds(idleDuration);
                    currentState = ArcherState.PatrolDown;
                    break;

                case ArcherState.PatrolDown:
                    animator.SetBool("isRunning", true);
                    yield return MoveTo(startPosition - new Vector3(0f, patrolDistance, 0f), moveDuration);
                    currentState = ArcherState.ReturnDown;
                    break;

                case ArcherState.ReturnDown:
                    animator.SetBool("isRunning", true);
                    yield return MoveTo(startPosition, moveDuration);
                    currentState = ArcherState.Idle;
                    break;

                case ArcherState.Attack:
                    animator.SetBool("isRunning", false);
                    yield return null; // stay here until Update() changes state
                    break;
            }
        }
    }

    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (currentState == ArcherState.Attack) yield break; // stop moving immediately

            elapsed += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            yield return null;
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
