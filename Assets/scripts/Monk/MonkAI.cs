using UnityEngine;
using System.Collections;

public class MonkAI : MonoBehaviour
{
    [Header("Detection Ranges")]
    public float fleeRange = 4f;      // Player this close → monk runs away
    public float prayRange = 6f;      // Player this close → monk prays
    public float safeRange = 8f;      // Player this far → monk returns to patrol

    [Header("Movement")]
    public float patrolSpeed = 1.2f;
    public float fleeSpeed = 3.5f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitAtPointDuration = 2f;
    public float patrolReachDistance = 0.3f;

    // Components
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State machine
    private enum MonkState { Patrol, Pray, Flee }
    private MonkState currentState = MonkState.Patrol;

    // Patrol tracking
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private Vector2 movement;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Monk: No Player found! Make sure Player tag is set.");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentState = MonkState.Patrol;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case MonkState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;

            case MonkState.Pray:
                HandlePray(distanceToPlayer);
                break;

            case MonkState.Flee:
                HandleFlee(distanceToPlayer);
                break;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    // ─────────────────────────────────────────
    //  PATROL
    // ─────────────────────────────────────────
    void HandlePatrol(float distanceToPlayer)
    {
        // Player very close → flee immediately
        if (distanceToPlayer <= fleeRange)
        {
            StopAllCoroutines();
            isWaiting = false;
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            animator.SetBool("isPreying", false);
            currentState = MonkState.Flee;
            return;
        }

        // Player nearby but not too close → pray
        if (distanceToPlayer <= prayRange)
        {
            StopAllCoroutines();
            isWaiting = false;
            movement = Vector2.zero;
            currentState = MonkState.Pray;
            return;
        }

        // No patrol points set → idle in place
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            animator.SetBool("isPreying", false);
            return;
        }

        if (!isWaiting)
        {
            Transform target = patrolPoints[currentPatrolIndex];
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            movement = direction * patrolSpeed;

            animator.SetBool("isPreying", false);
            animator.SetBool("isRunning", true);

            FlipSprite(direction.x);

            // Reached patrol point
            float distToPoint = Vector2.Distance(transform.position, target.position);
            if (distToPoint <= patrolReachDistance)
            {
                movement = Vector2.zero;
                StartCoroutine(WaitAtPoint());
            }
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        movement = Vector2.zero;
        animator.SetBool("isRunning", false);
        animator.SetBool("isPreying", false);

        yield return new WaitForSeconds(waitAtPointDuration);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    // ─────────────────────────────────────────
    //  PRAY
    // ─────────────────────────────────────────
    void HandlePray(float distanceToPlayer)
    {
        // Player got too close while praying → flee
        if (distanceToPlayer <= fleeRange)
        {
            animator.SetBool("isPreying", false);
            movement = Vector2.zero;
            currentState = MonkState.Flee;
            return;
        }

        // Player moved far away → back to patrol
        if (distanceToPlayer > safeRange)
        {
            animator.SetBool("isPreying", false);
            currentState = MonkState.Patrol;
            return;
        }

        // Stay still and pray facing away from player
        movement = Vector2.zero;
        animator.SetBool("isRunning", false);
        animator.SetBool("isPreying", true);

        // Face away from player while praying
        Vector2 dirAwayFromPlayer = (transform.position - player.position).normalized;
        FlipSprite(-dirAwayFromPlayer.x);
    }

    // ─────────────────────────────────────────
    //  FLEE
    // ─────────────────────────────────────────
    void HandleFlee(float distanceToPlayer)
    {
        // Reached safe distance → back to patrol
        if (distanceToPlayer >= safeRange)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            currentState = MonkState.Patrol;
            return;
        }

        // Run away from player
        Vector2 fleeDirection = (transform.position - player.position).normalized;
        movement = fleeDirection * fleeSpeed;

        animator.SetBool("isPreying", false);
        animator.SetBool("isRunning", true);

        FlipSprite(fleeDirection.x);
    }

    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────
    void FlipSprite(float directionX)
    {
        if (directionX < 0)
            spriteRenderer.flipX = false;
        else if (directionX > 0)
            spriteRenderer.flipX = true;
    }

    void OnDrawGizmosSelected()
    {
        // Flee range - red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);

        // Pray range - blue
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, prayRange);

        // Safe range - green
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeRange);

        // Patrol path - cyan lines
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
                int next = (i + 1) % patrolPoints.Length;
                if (patrolPoints[next] != null)
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
            }
        }
    }
}
