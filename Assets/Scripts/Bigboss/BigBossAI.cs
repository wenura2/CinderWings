using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BigBossAI : MonoBehaviour
{
    public enum BossStage { Stage1, Stage2, Stage3 }

    public bool HasAliveTroops()
{
    return aliveTroops.Count > 0;
}

    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;   // auto-find if empty
    public BigBossHealth health;           // ✅ reads HP from BigBossHealth

    [Header("Flip (2D left/right)")]
    public bool flipOnlySprite = true;
    private bool facingRight = true;

    [Header("Stage thresholds (percent HP)")]
    [Range(0f, 1f)] public float stage2AtPercent = 0.66f;
    [Range(0f, 1f)] public float stage3AtPercent = 0.33f;

    [Header("Distances & Movement")]
    public float attackRange = 2.2f;       // when boss decides to attack
    public float keepAwayDistance = 7f;    // stage3 while troops alive
    public float moveSpeed = 3.5f;

    [Header("Attack Timing")]
    public float attackCooldown = 1.2f;
    public float attackWindup = 0.25f;     // time until hit happens
    public float attackLockTime = 0.55f;   // total attack lock time

    [Header("Boss Damage To Player")]
    public float hitRadius = 1.6f;         // overlap circle radius for hit
    public LayerMask playerLayer;          // set to Player layer
    public int attack1Damage = 10;
    public int attack2Damage = 20;

    [Header("Hit Position (Optional)")]
    public Transform hitPoint;             // if null uses boss position

    [Header("Jump Away")]
    public float jumpAwayForce = 10f;
    public float jumpAwayDuration = 0.25f;
    public float jumpAwayRecover = 0.35f;

    [Header("Animator Params")]
    public string walkBool = "IsWalking";
    public string attack1Trigger = "Attack1";
    public string attack2Trigger = "Attack2";

    [Header("Stage 3 Troops")]
    public GameObject troopPrefab;
    public int troopsToSpawn = 4;
    public float spawnRadius = 4f;
    public float spawnCooldown = 10f;
    public LayerMask spawnBlockers;
    public float spawnCheckRadius = 0.25f;

    [Header("Stage 3 Rules")]
    public bool bossInvincibleWhileTroopsAlive = true;
    public bool bossStopsCompletelyWhileTroopsAlive = false;

    // Runtime
    public BossStage CurrentStage { get; private set; } = BossStage.Stage1;

    private bool isBusy;
    private float nextAttackTime;
    private float nextSpawnAllowedTime;
    private readonly List<GameObject> aliveTroops = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!health) health = GetComponent<BigBossHealth>();

        // helpful default: if you didn't set playerLayer, try to use "Player" layer
        if (playerLayer.value == 0)
        {
            int pLayer = LayerMask.NameToLayer("Player");
            if (pLayer != -1) playerLayer = 1 << pLayer;
        }
    }

    void Update()
    {
        if (!player) return;
        if (health != null && health.IsDead()) { StopMove(); return; }

        CleanupTroopList();
        UpdateStageByHealth();
        HandleFlip();

        // Stage 3: while troops alive => boss stays away and does NOT attack
        if (CurrentStage == BossStage.Stage3 && aliveTroops.Count > 0)
        {
            if (bossStopsCompletelyWhileTroopsAlive) StopMove();
            else KeepAwayFromPlayer();
            return;
        }

        if (isBusy) return;

        float dist = Vector2.Distance(rb.position, player.position);

        if (dist > attackRange)
            ChasePlayer();
        else
            TryAttack();
    }

    // -------------------------
    // Stage (reads from BigBossHealth)
    // -------------------------
    void UpdateStageByHealth()
    {
        if (!health) return;

        float hp01 = health.GetHealthPercent01();

        if (hp01 <= stage3AtPercent) CurrentStage = BossStage.Stage3;
        else if (hp01 <= stage2AtPercent) CurrentStage = BossStage.Stage2;
        else CurrentStage = BossStage.Stage1;
    }

    // -------------------------
    // Flip
    // -------------------------
    void HandleFlip()
    {
        float dx = player.position.x - transform.position.x;

        if (dx > 0.01f && !facingRight) Flip();
        else if (dx < -0.01f && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        if (flipOnlySprite && spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            return;
        }

        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // -------------------------
    // Movement
    // -------------------------
    void ChasePlayer()
    {
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (animator && !string.IsNullOrWhiteSpace(walkBool))
            animator.SetBool(walkBool, rb.velocity.sqrMagnitude > 0.01f);
    }

    void KeepAwayFromPlayer()
    {
        float dist = Vector2.Distance(rb.position, player.position);
        Vector2 away = (rb.position - (Vector2)player.position).normalized;

        if (dist < keepAwayDistance)
        {
            rb.velocity = away * moveSpeed;

            if (animator && !string.IsNullOrWhiteSpace(walkBool))
                animator.SetBool(walkBool, true);
        }
        else
        {
            StopMove();
        }
    }

    void StopMove()
    {
        rb.velocity = Vector2.zero;
        if (animator && !string.IsNullOrWhiteSpace(walkBool))
            animator.SetBool(walkBool, false);
    }

    // -------------------------
    // Attacks
    // -------------------------
    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isBusy = true;
        StopMove();

        // Stage 1 => Attack1 only
        // Stage 2/3 => Attack1 or Attack2
        bool useAttack1 = (CurrentStage == BossStage.Stage1) ? true : (Random.value < 0.5f);

        // Play animation
        if (animator)
        {
            string trig = useAttack1 ? attack1Trigger : attack2Trigger;
            if (!string.IsNullOrWhiteSpace(trig))
                animator.SetTrigger(trig);
        }

        // Windup: wait until the exact "hit frame"
        yield return new WaitForSeconds(attackWindup);

        // APPLY DAMAGE TO PLAYER (OverlapCircle like your PlayerController)
        DoHitPlayer(useAttack1 ? attack1Damage : attack2Damage);

        // Finish lock
        float remaining = Mathf.Max(0f, attackLockTime - attackWindup);
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        // Jump away after attacking (all stages)
        yield return JumpAwayFromPlayer();

        // Stage 3: spawn troops (cooldown protected)
        if (CurrentStage == BossStage.Stage3 && troopPrefab && Time.time >= nextSpawnAllowedTime)
        {
            yield return SpawnTroopsRoutine();
            nextSpawnAllowedTime = Time.time + spawnCooldown;
        }

        nextAttackTime = Time.time + attackCooldown;
        isBusy = false;
    }

    void DoHitPlayer(int damage)
    {
        Vector2 center = hitPoint ? (Vector2)hitPoint.position : (Vector2)transform.position;

        Collider2D hit = Physics2D.OverlapCircle(center, hitRadius, playerLayer);
        if (!hit) return;

        // Try PlayerHealth first (recommended)
        PlayerHealth ph = hit.GetComponent<PlayerHealth>();
        if (ph != null && !ph.IsDead())
        {
            ph.TakeDamage(damage);
            return;
        }

        // Fallback: if you have a different script name for health
        // hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    IEnumerator JumpAwayFromPlayer()
    {
        Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;

        float t = 0f;
        while (t < jumpAwayDuration)
        {
            t += Time.deltaTime;
            rb.velocity = away * jumpAwayForce;
            yield return null;
        }

        StopMove();
        yield return new WaitForSeconds(jumpAwayRecover);
    }

    // -------------------------
    // Troops
    // -------------------------
    IEnumerator SpawnTroopsRoutine()
    {
        StopMove();

        for (int i = 0; i < troopsToSpawn; i++)
        {
            Vector2 pos = FindValidSpawnPosition();
            GameObject troop = Instantiate(troopPrefab, pos, Quaternion.identity);
            aliveTroops.Add(troop);
            yield return new WaitForSeconds(0.05f);
        }
    }

    Vector2 FindValidSpawnPosition()
    {
        Vector2 center = transform.position;

        for (int tries = 0; tries < 40; tries++)
        {
            Vector2 pos = center + Random.insideUnitCircle * spawnRadius;
            if (!Physics2D.OverlapCircle(pos, spawnCheckRadius, spawnBlockers))
                return pos;
        }

        return center + (Random.insideUnitCircle.normalized * (spawnRadius * 0.6f));
    }

    void CleanupTroopList()
    {
        for (int i = aliveTroops.Count - 1; i >= 0; i--)
        {
            if (aliveTroops[i] == null)
                aliveTroops.RemoveAt(i);
        }
    }

    // -------------------------
    // OPTIONAL wrapper (if your player still hits BigBossAI)
    // -------------------------
    public void TakeDamage(int amount)
    {
        if (!health) return;
        if (health.IsDead()) return;

        if (bossInvincibleWhileTroopsAlive && CurrentStage == BossStage.Stage3 && aliveTroops.Count > 0)
            return;

        health.TakeDamage(amount);
    }

    // Debug
    [ContextMenu("Debug: Damage 50")]
    void DebugDamage50() => TakeDamage(50);

    void OnDrawGizmosSelected()
    {
        Vector3 center = hitPoint ? hitPoint.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, hitRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}