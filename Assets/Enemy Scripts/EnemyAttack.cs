using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public int damage = 1;
    public float attackCooldown = 1.0f;

    [Header("Front-Only Spear Hitbox")]
    public Vector2 hitBoxSize = new Vector2(1.2f, 0.6f);  // width, height of hit zone
    public float hitBoxForwardOffset = 0.8f;              // how far in front of enemy
    public LayerMask playerMask;

    [Header("Target")]
    public Transform player;

    [Header("Animation")]
    public Animator animator;
    public string attackTriggerName = "Attack";

    [Header("Facing Source")]
    public Rigidbody2D rb;
    public Vector2 idleFacing = Vector2.up;               // default facing when standing still

    private float cooldownTimer = 0f;
    private Vector2 lastFacing;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        lastFacing = idleFacing.normalized;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        // Keep track of last facing direction from movement
        if (rb && rb.linearVelocity.sqrMagnitude > 0.01f)
            lastFacing = rb.linearVelocity.normalized;
    }

    /// <summary>
    /// Called by EnemyAI. Starts attack animation if in range and off cooldown.
    /// Damage is applied via Animation Event -> AnimEvent_DealDamage().
    /// </summary>
    public bool TryAttack()
    {
        if (player == null) return false;
        if (cooldownTimer > 0f) return false;

        // Simple range gate so enemy doesn't attack from far away
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > hitBoxForwardOffset + (hitBoxSize.x * 0.6f)) return false;

        cooldownTimer = attackCooldown;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        return true;
    }

    /// <summary>
    /// Add an Animation Event on the exact "impact" frame, calling this.
    /// </summary>
    public void AnimEvent_DealDamage()
    {
        ApplyHitFrontOnly();
    }

    private void ApplyHitFrontOnly()
    {
        Vector2 facing = (lastFacing.sqrMagnitude < 0.0001f) ? idleFacing.normalized : lastFacing.normalized;

        // Center of hitbox in front of enemy
        Vector2 center = (Vector2)transform.position + facing * hitBoxForwardOffset;

        // Rotate hitbox to match facing
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        // Get all targets in the spear box
        Collider2D hit = Physics2D.OverlapBox(center, hitBoxSize, angle, playerMask);
        if (!hit) return;

        EggHealth egg = hit.GetComponentInParent<EggHealth>();
        if (!egg) egg = hit.GetComponent<EggHealth>();
        if (!egg) return;

        // Hit direction pushes egg away from enemy toward facing direction
        Vector2 hitDir = facing;
        egg.TakeDamage(damage, hitDir);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector2 facing = idleFacing.normalized;

        if (Application.isPlaying && lastFacing.sqrMagnitude > 0.01f)
            facing = lastFacing.normalized;

        Vector2 center = (Vector2)transform.position + facing * hitBoxForwardOffset;
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        Gizmos.color = Color.red;

        // Draw rotated box gizmo
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(hitBoxSize.x, hitBoxSize.y, 0));
        Gizmos.matrix = old;
    }
#endif
}