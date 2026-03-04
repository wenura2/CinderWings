using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public int attackDamage = 12;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealthM3 enemyHealth;

    private Vector2 movement;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealthM3>();
    }

    void Update()
    {
        if (player == null || enemyHealth == null || enemyHealth.IsDead()) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (distance <= attackRange)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);

            if (attackTimer <= 0 && !isAttacking)
                StartAttack();
        }
        else if (distance <= detectionRange)
        {
            if (!isAttacking)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                movement = direction;
                animator.SetFloat("Speed", movement.magnitude);

                // Flip sprite for facing direction
                spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    void FixedUpdate()
    {
        // ✅ Only move if not attacking and not knocked back
        if (!isAttacking && !enemyHealth.IsKnockedBack())
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
        // ❌ Removed the else block that was cancelling knockback velocity
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // Reset triggers
        animator.ResetTrigger("AttackRight");
        animator.ResetTrigger("AttackUp");
        animator.ResetTrigger("AttackDown");

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Quadrant-based attack selection (Right, Up, Down only)
        if (angle > -45 && angle <= 45)
        {
            animator.SetTrigger("AttackRight");
        }
        else if (angle > 45 && angle <= 135)
        {
            animator.SetTrigger("AttackUp");
        }
        else
        {
            animator.SetTrigger("AttackDown");
        }

        Invoke("DealDamage", 0.3f); // sync damage with animation
    }

    void DealDamage()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.5f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Enemy dealt " + attackDamage + " damage!");
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
}
