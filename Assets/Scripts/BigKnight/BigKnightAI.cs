using UnityEngine;
using System.Collections;

public class BigKnightAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public int attackDamage = 35;

    [Header("Animator State Names")]
    public string idleStateName = "BK_Idle";
    public string runStateName = "BK_Run";
    public string attackStateName = "BK_Attack";
    public string hurtStateName = "BK_Hurt";
    public string deadStateName = "BK_Dead";

    // Components
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State machine
    private enum KnightState { Idle, Chase, Attack, Hurt, Dead }
    private KnightState currentState = KnightState.Idle;

    // Tracking
    private Vector2 movement;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("BigKnight: No Player found! Set Player tag.");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (currentState == KnightState.Dead) return;
        if (player == null) return;

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Auto reset attack flag when animation finishes
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool attackPlaying = stateInfo.IsName(attackStateName);
        if (isAttacking && !attackPlaying)
            isAttacking = false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case KnightState.Idle:
                HandleIdle(distanceToPlayer);
                break;

            case KnightState.Chase:
                HandleChase(distanceToPlayer);
                break;

            case KnightState.Attack:
                HandleAttack(distanceToPlayer);
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == KnightState.Dead) return;

        if (!isAttacking)
            rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
        else
            rb.linearVelocity = Vector2.zero;
    }

    // ─────────────────────────────────────────
    //  IDLE - wait until player enters range
    // ─────────────────────────────────────────
    void HandleIdle(float distanceToPlayer)
    {
        movement = Vector2.zero;
        animator.SetBool("isRunning", false);

        if (distanceToPlayer <= detectionRange)
        {
            currentState = KnightState.Chase;
        }
    }

    // ─────────────────────────────────────────
    //  CHASE
    // ─────────────────────────────────────────
    void HandleChase(float distanceToPlayer)
    {
        // Player escaped → go back to idle
        if (distanceToPlayer > detectionRange)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            currentState = KnightState.Idle;
            return;
        }

        // Close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            currentState = KnightState.Attack;
            return;
        }

        // Chase player
        if (!isAttacking)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = direction * moveSpeed;

            animator.SetBool("isRunning", true);
            FaceDirection(direction.x);
        }
    }

    // ─────────────────────────────────────────
    //  ATTACK
    // ─────────────────────────────────────────
    void HandleAttack(float distanceToPlayer)
    {
        // Player moved away → chase again
        if (distanceToPlayer > attackRange)
        {
            currentState = KnightState.Chase;
            return;
        }

        movement = Vector2.zero;
        animator.SetBool("isRunning", false);

        if (attackTimer <= 0 && !isAttacking)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        // Wait for swing moment then deal damage
        yield return new WaitForSeconds(0.4f);

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange + 0.5f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(attackDamage);
                    Debug.Log("BigKnight hit player for " + attackDamage);
                }
                else
                    Debug.LogWarning("PlayerHealth not found on Player!");
            }
        }
    }

    // ─────────────────────────────────────────
    //  TAKE DAMAGE (called by BigKnightHealth)
    // ─────────────────────────────────────────
    public void OnHurt()
    {
        if (currentState == KnightState.Dead) return;
        StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        currentState = KnightState.Hurt;
        movement = Vector2.zero;

        animator.SetBool("isHurt", true);
        yield return new WaitForSeconds(0.4f);
        animator.SetBool("isHurt", false);

        if (currentState != KnightState.Dead)
            currentState = KnightState.Chase;
    }

    // ─────────────────────────────────────────
    //  DEATH
    // ─────────────────────────────────────────
    public void SetDead()
    {
        currentState = KnightState.Dead;
        StopAllCoroutines();
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isRunning", false);
        animator.SetBool("isHurt", false);
        animator.SetBool("isDead", true);
    }

    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────
    void FaceDirection(float directionX)
    {
        if (directionX < 0)
            spriteRenderer.flipX = true;
        else if (directionX > 0)
            spriteRenderer.flipX = false;
    }

    void OnDrawGizmosSelected()
    {
        // Detection range - yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range - red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}