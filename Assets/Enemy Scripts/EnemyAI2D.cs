// ========================= EnemyAI2D.cs (UPDATED) =========================
// Patrol -> Alert -> Chase (Attack) -> Lose -> Return same path -> Patrol
// Works with EnemyAttack + EnemyAnimationController + Rigidbody2D velocity (Speed)

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Return }

    [Header("References")]
    public Transform player;
    public LayerMask obstacleMask; // for LOS raycast only
    public EnemyAnimationController animController;
    public EnemyAttack attack;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1.8f;
    public float waitAtPoint = 1.0f;
    public float pointReachDistance = 0.15f;

    [Header("Detection")]
    public float detectRadius = 4.5f;
    public float loseRadius = 6.5f;
    public bool requireLineOfSight = true;

    [Header("Alert")]
    public float alertDuration = 0.4f;

    [Header("Chase/Return")]
    public float chaseSpeed = 2.6f;
    public float returnSpeed = 2.0f;

    [Header("Return (breadcrumbs)")]
    public float breadcrumbInterval = 0.2f;
    public float breadcrumbMinDistance = 0.25f;

    [Header("Movement Smoothing")]
    public float steering = 10f;

    private Rigidbody2D rb;
    private State state = State.Patrol;

    private int patrolIndex = 0;
    private float waitTimer = 0f;

    private Vector2 desiredVel;

    private bool hasAlertedThisDetection = false;
    private float alertTimer = 0f;

    private readonly List<Vector2> breadcrumbs = new();
    private float breadcrumbTimer = 0f;

    private bool savedPatrolResume = false;
    private int savedPatrolIndex = 0;
    private float savedWaitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        if (!animController) animController = GetComponent<EnemyAnimationController>();
        if (!attack) attack = GetComponent<EnemyAttack>();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            Patrol();
            ApplySteering(patrolSpeed);
            return;
        }

        bool seesPlayer = CanDetectPlayer();
        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                if (seesPlayer)
                {
                    if (!hasAlertedThisDetection) EnterAlert();
                    else EnterChase();
                }
                else
                {
                    hasAlertedThisDetection = false;
                    Patrol();
                }
                ApplySteering(patrolSpeed);
                break;

            case State.Alert:
                desiredVel = Vector2.zero;
                rb.velocity = Vector2.zero; // make Speed = 0
                alertTimer -= Time.fixedDeltaTime;
                if (alertTimer <= 0f) EnterChase();
                break;

            case State.Chase:
                if (!seesPlayer || dist > loseRadius)
                {
                    EnterReturn();
                    ApplySteering(returnSpeed);
                    break;
                }

                RecordBreadcrumb();

                // Try attack when close
                bool didAttack = false;
                if (attack != null)
                {
                    attack.player = player;
                    didAttack = attack.TryAttack();
                }

                if (didAttack)
                {
                    // Stop while attacking (optional)
                    desiredVel = Vector2.zero;
                    ApplySteering(0f);
                    break;
                }

                // Chase movement
                Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
                desiredVel = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized * chaseSpeed : Vector2.zero;

                ApplySteering(chaseSpeed);
                break;

            case State.Return:
                // If player comes back into sight, re-engage
                if (seesPlayer && dist <= loseRadius)
                {
                    if (!hasAlertedThisDetection) EnterAlert();
                    else EnterChase();
                    break;
                }

                ReturnAlongBreadcrumbs();
                ApplySteering(returnSpeed);
                break;
        }
    }

    // ---------------- transitions ----------------

    void EnterAlert()
    {
        state = State.Alert;
        alertTimer = alertDuration;
        hasAlertedThisDetection = true;

        if (animController) animController.PlayAlert();
    }

    void EnterChase()
    {
        if (!savedPatrolResume)
        {
            savedPatrolResume = true;
            savedPatrolIndex = patrolIndex;
            savedWaitTimer = waitTimer;
        }

        state = State.Chase;

        breadcrumbs.Clear();
        breadcrumbs.Add(transform.position);
        breadcrumbTimer = 0f;
    }

    void EnterReturn()
    {
        state = State.Return;
        if (breadcrumbs.Count == 0) breadcrumbs.Add(transform.position);
    }

    void FinishReturnToPatrol()
    {
        state = State.Patrol;
        desiredVel = Vector2.zero;

        if (savedPatrolResume)
        {
            patrolIndex = savedPatrolIndex;
            waitTimer = savedWaitTimer;
            savedPatrolResume = false;
        }

        hasAlertedThisDetection = false;
    }

    // ---------------- behavior ----------------

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            desiredVel = Vector2.zero;
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            desiredVel = Vector2.zero;
            return;
        }

        Vector2 target = patrolPoints[patrolIndex].position;
        Vector2 toTarget = target - (Vector2)transform.position;

        if (toTarget.magnitude <= pointReachDistance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            waitTimer = waitAtPoint;
            desiredVel = Vector2.zero;
            return;
        }

        desiredVel = toTarget.normalized * patrolSpeed;
    }

    void RecordBreadcrumb()
    {
        breadcrumbTimer -= Time.fixedDeltaTime;
        if (breadcrumbTimer > 0f) return;

        breadcrumbTimer = breadcrumbInterval;

        Vector2 pos = transform.position;
        if (breadcrumbs.Count == 0)
        {
            breadcrumbs.Add(pos);
            return;
        }

        if (Vector2.Distance(breadcrumbs[breadcrumbs.Count - 1], pos) >= breadcrumbMinDistance)
            breadcrumbs.Add(pos);
    }

    void ReturnAlongBreadcrumbs()
    {
        if (breadcrumbs.Count == 0)
        {
            FinishReturnToPatrol();
            return;
        }

        Vector2 target = breadcrumbs[breadcrumbs.Count - 1];
        Vector2 toTarget = target - (Vector2)transform.position;

        if (toTarget.magnitude <= pointReachDistance)
        {
            breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
            if (breadcrumbs.Count == 0) FinishReturnToPatrol();
            desiredVel = Vector2.zero;
            return;
        }

        desiredVel = toTarget.normalized * returnSpeed;
    }

    // ---------------- movement ----------------

    void ApplySteering(float maxSpeed)
    {
        if (maxSpeed <= 0.001f)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, steering * Time.fixedDeltaTime);
            return;
        }

        Vector2 newVel = Vector2.Lerp(rb.velocity, desiredVel, steering * Time.fixedDeltaTime);

        if (newVel.magnitude > maxSpeed)
            newVel = newVel.normalized * maxSpeed;

        rb.velocity = newVel;
    }

    // ---------------- detection ----------------

    bool CanDetectPlayer()
    {
    if (player == null) return false;

    EggHealth egg = player.GetComponent<EggHealth>();
    if (egg != null && egg.isHidden)
        return false;   // Cannot see hidden player


        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;

        if (toPlayer.magnitude > detectRadius) return false;
        if (!requireLineOfSight) return true;

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
        return hit.collider == null;
    }
}