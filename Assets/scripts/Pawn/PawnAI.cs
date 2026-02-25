using UnityEngine;

public class PawnAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Movement")]
    public float runSpeed = 3f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1.2f;
    public float waitAtPointDuration = 1.5f;
    public float patrolReachDistance = 0.3f;

    // Components
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // State
    private enum PawnState { Patrol, RunAway }
    private PawnState currentState = PawnState.Patrol;

    // Patrol
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private Vector2 movement;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Pawn: No Player found! Make sure Player tag is set.");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case PawnState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;

            case PawnState.RunAway:
                HandleRunAway(distanceToPlayer);
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
        if (distanceToPlayer <= detectionRange)
        {
            StopAllCoroutines();
            isWaiting = false;
            movement = Vector2.zero;
            currentState = PawnState.RunAway;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            return;
        }

        if (!isWaiting)
        {
            Transform target = patrolPoints[currentPatrolIndex];
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            movement = direction * patrolSpeed;

            animator.SetBool("isRunning", true);

            // Face the direction of movement during patrol
            FaceDirection(direction.x);

            float distToPoint = Vector2.Distance(transform.position, target.position);
            if (distToPoint <= patrolReachDistance)
            {
                movement = Vector2.zero;
                StartCoroutine(WaitAtPoint());
            }
        }
    }

    System.Collections.IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        movement = Vector2.zero;
        animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(waitAtPointDuration);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    // ─────────────────────────────────────────
    //  RUN AWAY
    // ─────────────────────────────────────────
    void HandleRunAway(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            currentState = PawnState.Patrol;
            return;
        }

        // Direction AWAY from player
        Vector2 direction = (transform.position - player.position).normalized;
        movement = direction * runSpeed;

        animator.SetBool("isRunning", true);

        // Face the direction pawn is actually running (away from player)
        // So we use direction.x directly — pawn looks where it runs
        FaceDirection(direction.x);
    }

    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────

    // Pass the direction the character is MOVING
    // and it will face that direction correctly
    void FaceDirection(float directionX)
    {
        if (directionX < 0)
            spriteRenderer.flipX = true;   // moving left → face left
        else if (directionX > 0)
            spriteRenderer.flipX = false;  // moving right → face right
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

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