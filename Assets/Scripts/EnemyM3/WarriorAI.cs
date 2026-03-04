using UnityEngine;

public class WarriorAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 2f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public int attackDamage = 15;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private float attackTimer = 0f;
    private bool isAttacking = false; // internal flag only

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

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

                // Flip sprite depending on facing direction
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
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // Only trigger the attack animation
        animator.SetTrigger("Attack");

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
                Debug.Log("Warrior dealt " + attackDamage + " damage!");
            }
        }
    }

    // Called at the end of attack animation via Animation Event
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
}
