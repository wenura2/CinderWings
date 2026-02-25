using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float fireBreathMoveMultiplier = 0.6f;

    [Header("Attack 1 (Melee)")]
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 20;

    [Header("Attack 2 (Firebreath Burst)")]
    [SerializeField] private float fireBreathDamagePerSecond = 25f;
    [SerializeField] private float fireBreathTickRate = 0.08f;
    [SerializeField] private float fireBurstDuration = 1.0f;

    [Header("Firebreath Shape (Front Only)")]
    [SerializeField] private Vector2 fireBreathBoxSize = new Vector2(2.4f, 1.4f);
    [SerializeField] private float fireBreathForwardOffset = 1.2f;

    [Header("Firebreath Meter")]
    [SerializeField] private float fireBreathMax = 100f;
    [SerializeField] private float fireBreathDrainPerSecond = 20f;
    [SerializeField] private float fireBreathRefillOnKill = 25f;

    [Header("Combat Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private PlayerHealth playerHealth;

    private Vector3 lastMousePosition;

    private bool isAttacking;
    private float attackTimer;

    private bool isFireBreathing;
    private float fireBreathMeter;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        fireBreathMeter = fireBreathMax;

        if (Mouse.current != null)
            lastMousePosition = Mouse.current.position.ReadValue();
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead()) return;

        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        if (myAnimator)
        {
            if (!isAttacking && !isFireBreathing)
                myAnimator.SetBool("isMoving", movement != Vector2.zero);
            else
                myAnimator.SetBool("isMoving", false);
        }

        FaceMouseOnlyIfMoved();

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        // Attack 1 (Left click) - melee
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isAttacking && !isFireBreathing && attackTimer <= 0f)
                StartCoroutine(MeleeAttackRoutine());
        }

        // Attack 2 (Right click) - single fire burst (front only)
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (!isAttacking && !isFireBreathing && fireBreathMeter > 0f)
                StartCoroutine(FireBreathBurstRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead()) return;

        float speed = moveSpeed;
        if (isFireBreathing) speed *= fireBreathMoveMultiplier;

        if (!isAttacking)
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        else
            rb.linearVelocity = Vector2.zero;
    }

    // =========================
    // Attack 1 (Melee)
    // =========================
    private IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        if (myAnimator)
        {
            myAnimator.SetBool("isMoving", false);
            myAnimator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.2f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
            ApplyDamageAndHandleKillRefill(hit, attackDamage);

        yield return new WaitForSeconds(Mathf.Max(0f, attackCooldown - 0.2f));
        isAttacking = false;
    }

    // =========================
    // Attack 2 (Firebreath) - SINGLE BURST (FRONT ONLY)
    // =========================
    private IEnumerator FireBreathBurstRoutine()
    {
        isFireBreathing = true;

        if (myAnimator)
        {
            myAnimator.SetTrigger("Attack2");
            myAnimator.SetBool("isFireBreathing", true);
        }

        float tick = Mathf.Max(0.02f, fireBreathTickRate);
        float timer = 0f;

        while (timer < fireBurstDuration && fireBreathMeter > 0f)
        {
            if (playerHealth != null && playerHealth.IsDead())
                break;

            timer += tick;

            // Drain meter
            fireBreathMeter -= fireBreathDrainPerSecond * tick;
            fireBreathMeter = Mathf.Clamp(fireBreathMeter, 0f, fireBreathMax);

            // DPS -> per tick
            int dmgThisTick = Mathf.CeilToInt(fireBreathDamagePerSecond * tick);

            // Front-only box hit
            Vector2 boxCenter = GetFireBoxCenter();
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, fireBreathBoxSize, 0f, enemyLayer);
            foreach (Collider2D hit in hits)
                ApplyDamageAndHandleKillRefill(hit, dmgThisTick);

            yield return new WaitForSeconds(tick);
        }

        isFireBreathing = false;
        if (myAnimator) myAnimator.SetBool("isFireBreathing", false);
    }

    private Vector2 GetFireBoxCenter()
    {
        // Your flip logic: flipX true when mouse is to the right -> treat as facing RIGHT
        float dirX = (mySpriteRenderer != null && mySpriteRenderer.flipX) ? 1f : -1f;
        return (Vector2)transform.position + new Vector2(dirX * fireBreathForwardOffset, 0f);
    }

    // =========================
    // Damage + refill on kill
    // =========================
    private void ApplyDamageAndHandleKillRefill(Collider2D hit, int damage)
    {
        BreakableHouse house = hit.GetComponent<BreakableHouse>();
        if (house != null)
        {
            house.RegisterHit();
            return;
        }

        EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
if (enemyHealth != null)
{
    bool wasDead = TryGetDeadState(enemyHealth, out bool d0) && d0;
    // ✅ Pass transform.position so knockback + dust work
    enemyHealth.TakeDamage(damage, transform.position);
    bool isDeadNow = TryGetDeadState(enemyHealth, out bool d1) && d1;
    if (!wasDead && isDeadNow) RefillFireBreathOnKill();
    return;
}


        BigKnightHealth bkHealth = hit.GetComponent<BigKnightHealth>();
        if (bkHealth != null)
        {
            bool wasDead = TryGetDeadState(bkHealth, out bool d0) && d0;
            bkHealth.TakeDamage(damage);
            bool isDeadNow = TryGetDeadState(bkHealth, out bool d1) && d1;
            if (!wasDead && isDeadNow) RefillFireBreathOnKill();
            return;
        }

        BigBossHealth bossHealth = hit.GetComponent<BigBossHealth>();
        if (bossHealth != null)
        {
            bool wasDead = TryGetDeadState(bossHealth, out bool d0) && d0;
            bossHealth.TakeDamage(damage);
            bool isDeadNow = TryGetDeadState(bossHealth, out bool d1) && d1;
            if (!wasDead && isDeadNow) RefillFireBreathOnKill();
            return;
        }

        ArcherHealth archerHealth = hit.GetComponent<ArcherHealth>();
        if (archerHealth != null)
        {
            bool wasDead = TryGetDeadState(archerHealth, out bool d0) && d0;
            archerHealth.TakeDamage(damage, transform.position);
            bool isDeadNow = TryGetDeadState(archerHealth, out bool d1) && d1;
            if (!wasDead && isDeadNow) RefillFireBreathOnKill();
            return;
        }
    }

    private void RefillFireBreathOnKill()
    {
        fireBreathMeter += fireBreathRefillOnKill;
        fireBreathMeter = Mathf.Clamp(fireBreathMeter, 0f, fireBreathMax);
    }

    private bool TryGetDeadState(object obj, out bool dead)
    {
        dead = false;
        if (obj == null) return false;

        var t = obj.GetType();

        var m = t.GetMethod("IsDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m != null && m.ReturnType == typeof(bool) && m.GetParameters().Length == 0)
        {
            dead = (bool)m.Invoke(obj, null);
            return true;
        }

        var p = t.GetProperty("IsDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetProperty("isDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(bool) && p.GetIndexParameters().Length == 0)
        {
            dead = (bool)p.GetValue(obj);
            return true;
        }

        var f = t.GetField("isDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             ?? t.GetField("dead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool))
        {
            dead = (bool)f.GetValue(obj);
            return true;
        }

        return false;
    }

    private void FaceMouseOnlyIfMoved()
    {
        if (Mouse.current == null || mySpriteRenderer == null || Camera.main == null) return;

        Vector3 currentMousePosition = Mouse.current.position.ReadValue();
        if (currentMousePosition == lastMousePosition) return;

        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

        // Keeping your original behavior:
        // flipX true when mouse is to the right
        mySpriteRenderer.flipX = currentMousePosition.x > playerScreenPosition.x;

        lastMousePosition = currentMousePosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Vector2 center = Application.isPlaying ? GetFireBoxCenter() : (Vector2)transform.position;
        Gizmos.DrawWireCube(center, fireBreathBoxSize);
    }

    public float GetFireBreathMeter01() => fireBreathMax <= 0 ? 0 : fireBreathMeter / fireBreathMax;
}