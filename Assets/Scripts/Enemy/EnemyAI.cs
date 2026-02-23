using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Check if attack animation is still playing
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool attackAnimPlaying = stateInfo.IsName("Warrior_Attack1_Black") ||
                                  stateInfo.IsName("Warrior_Attack2_Black");

        // Auto-reset isAttacking when animation finishes
        // This is the BACKUP in case Animation Event is not set
        if (isAttacking && !attackAnimPlaying)
        {
            isAttacking = false;
        }

        if (distance <= attackRange)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);

            if (attackTimer <= 0 && !isAttacking)
            {
                StartAttack();
            }
        }
        else if (distance <= detectionRange)
        {
            if (!isAttacking)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                movement = direction;
                animator.SetFloat("Speed", movement.magnitude);

                if (direction.x < 0)
                    spriteRenderer.flipX = true;
                else if (direction.x > 0)
                    spriteRenderer.flipX = false;
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
        if (!isAttacking)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        else
            rb.linearVelocity = Vector2.zero; // Stop sliding during attack
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // Reset trigger first to avoid queuing
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        // Delay damage so it hits when animation swings
        Invoke("DealDamage", 0.3f);

        Debug.Log("Enemy attacked!");
    }

    void DealDamage()
    {
        // Check player still in range when damage fires
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.5f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Dealt " + attackDamage + " damage to player!");
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player!");
            }
        }
    }

    // Still keep this for Animation Event if you set it up
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
}