using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public BossAnimator bossAnim;
    public SpriteRenderer spriteRenderer;

    [Header("Health")]
    public int maxHealth = 200;
    public float hurtInvincibleTime = 0.25f;

    [Header("Hit VFX")]
    public GameObject hitVfxPrefab;
    public float hitVfxLifetime = 1.5f;
    public Transform hitVfxPoint; // optional

    [Header("Aggro (Chase only when in range)")]
    public float aggroRange = 10f;
    public bool keepChasingOnceAggro = true;
    public float deAggroRange = 12f;

    [Header("Movement")]
    public float moveSpeed = 1.6f;
    public float stopDistance = 2.5f;

    [Header("Flip")]
    public bool faceRightByDefault = true;
    public float flipDeadZone = 0.05f;

    [Header("Attack (Single Attack)")]
    public float attackRange = 2.8f;      // melee range for attack to happen
    public float attackCooldown = 2.0f;   // time between attacks
    public float attackWindup = 0.2f;     // delay before spawning enemies (animation windup)
    public bool requirePlayerInAttackRange = true;

    [Header("Attack Damage (Hitbox)")]
    public Collider2D attackHitbox;       // child collider (IsTrigger). Disabled by default.
    public int attackDamage = 20;
    public float hitboxActiveTime = 0.15f;

    [Header("Spawn Enemies During Attack")]
    public GameObject[] enemyPrefabs;
    public int enemiesPerAttack = 2;
    public int maxAliveEnemies = 8;
    public bool randomEnemy = true;

    [Header("Spawn Positions")]
    public Transform[] spawnPoints;       // optional
    public float spawnSpreadRadius = 1.25f;

    [Header("Optional VFX")]
    public GameObject spawnVFXPrefab;
    public float vfxLifetime = 2f;

    // internals
    private Rigidbody2D rb;
    private int currentHealth;
    private bool isDead;
    private bool invincible;
    private bool attackOnCooldown;
    private bool aggro;
    private int enemyIndex;

    private readonly List<GameObject> aliveSpawned = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        currentHealth = maxHealth;

        if (!bossAnim) bossAnim = GetComponent<BossAnimator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (attackHitbox) attackHitbox.enabled = false;
    }

    void Update()
    {
        if (isDead) return;

        CleanupAliveList();
        UpdateAggro();
        HandleMovementAndFlip();
        TryStartAttack();
    }

    // ---------------------------
    // Aggro / Movement / Flip
    // ---------------------------
    private void UpdateAggro()
    {
        if (player == null)
        {
            aggro = false;
            return;
        }

        float d = Vector2.Distance(transform.position, player.position);

        if (!aggro)
        {
            if (d <= aggroRange) aggro = true;
        }
        else
        {
            if (!keepChasingOnceAggro && d > deAggroRange)
                aggro = false;
        }
    }

    private void HandleMovementAndFlip()
    {
        if (!aggro || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            if (bossAnim) bossAnim.SetWalking(false);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // Stop when close enough (so we can attack)
        if (dist <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            if (bossAnim) bossAnim.SetWalking(false);

            // Face player while idle
            FacePlayerWhenIdle();
            return;
        }

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        if (bossAnim) bossAnim.SetWalking(rb.linearVelocity.sqrMagnitude > 0.01f);

        UpdateFlipFromVelocity();
    }

    private void FacePlayerWhenIdle()
    {
        if (!spriteRenderer || player == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;

        bool playerIsRight = dx > 0f;
        bool flipX = faceRightByDefault ? !playerIsRight : playerIsRight;
        spriteRenderer.flipX = flipX;
    }

    private void UpdateFlipFromVelocity()
    {
        if (!spriteRenderer) return;

        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) < flipDeadZone) return;

        bool movingRight = vx > 0f;
        bool flipX = faceRightByDefault ? !movingRight : movingRight;
        spriteRenderer.flipX = flipX;
    }

    // ---------------------------
    // Attack (Animation + Damage + Spawn)
    // ---------------------------
    private void TryStartAttack()
    {
        if (!aggro) return;
        if (attackOnCooldown) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= attackRange;

        if (requirePlayerInAttackRange && !inRange)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        attackOnCooldown = true;

        // Stop to attack
        rb.linearVelocity = Vector2.zero;
        if (bossAnim) bossAnim.SetWalking(false);

        // Play attack animation trigger
        if (bossAnim) bossAnim.PlayAttack();

        // Spawn enemies after windup (sync with animation)
        if (attackWindup > 0f)
            yield return new WaitForSeconds(attackWindup);

        SpawnEnemiesForThisAttack();

        // cooldown
        if (attackCooldown > 0f)
            yield return new WaitForSeconds(attackCooldown);

        attackOnCooldown = false;
    }

    private void SpawnEnemiesForThisAttack()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (CountAliveEnemies() >= maxAliveEnemies) return;

        for (int i = 0; i < enemiesPerAttack; i++)
        {
            if (CountAliveEnemies() >= maxAliveEnemies) break;

            GameObject prefab = PickEnemyPrefab();
            if (prefab == null) break;

            Vector3 pos = GetSpawnPosition();
            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
            aliveSpawned.Add(enemy);

            if (spawnVFXPrefab != null)
            {
                GameObject fx = Instantiate(spawnVFXPrefab, pos, Quaternion.identity);
                if (vfxLifetime > 0f) Destroy(fx, vfxLifetime);
            }
        }
    }

    private GameObject PickEnemyPrefab()
    {
        if (randomEnemy)
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject p = enemyPrefabs[enemyIndex % enemyPrefabs.Length];
        enemyIndex++;
        return p;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 basePos =
            (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
            : transform.position;

        Vector2 offset = Random.insideUnitCircle * spawnSpreadRadius;
        return basePos + new Vector3(offset.x, offset.y, 0f);
    }

    // ---- CALLED BY ANIMATION EVENT ----
    // Add an Animation Event on the hit frame that calls: EnableAttackHitbox
    public void EnableAttackHitbox()
    {
        if (isDead) return;
        if (attackHitbox == null) return;

        attackHitbox.enabled = true;
        StartCoroutine(DisableHitboxAfterTime());
    }

    private IEnumerator DisableHitboxAfterTime()
    {
        yield return new WaitForSeconds(hitboxActiveTime);

        if (attackHitbox)
            attackHitbox.enabled = false;
    }

    // Called from hitbox child script
    public void DamagePlayer(Collider2D other)
    {
        if (isDead) return;
        if (attackHitbox == null || !attackHitbox.enabled) return;
        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>(); // change if your health script name differs
        if (ph != null)
            ph.TakeDamage(attackDamage);
    }

    // ---------------------------
    // Health / Hurt / Death (+ Hit VFX)
    // ---------------------------
    public void TakeDamage(int damage)
    {
        if (isDead || invincible) return;

        // getting hit wakes the boss
        aggro = true;

        currentHealth -= damage;

        // 🔥 HIT VFX
        SpawnHitVfx();

        if (bossAnim) bossAnim.PlayHurt();

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(InvincibleRoutine());
    }

    private void SpawnHitVfx()
    {
        if (!hitVfxPrefab) return;

        Vector3 spawnPos = hitVfxPoint ? hitVfxPoint.position : transform.position;
        GameObject vfx = Instantiate(hitVfxPrefab, spawnPos, Quaternion.identity);

        if (hitVfxLifetime > 0f)
            Destroy(vfx, hitVfxLifetime);
    }

    private IEnumerator InvincibleRoutine()
    {
        invincible = true;
        yield return new WaitForSeconds(hurtInvincibleTime);
        invincible = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (attackHitbox) attackHitbox.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        StopAllCoroutines();

        if (bossAnim) bossAnim.PlayDeathAndFade();
        else Destroy(gameObject);
    }

    // ---------------------------
    // Spawned list cleanup
    // ---------------------------
    private int CountAliveEnemies()
    {
        CleanupAliveList();
        return aliveSpawned.Count;
    }

    private void CleanupAliveList()
    {
        for (int i = aliveSpawned.Count - 1; i >= 0; i--)
        {
            if (aliveSpawned[i] == null)
                aliveSpawned.RemoveAt(i);
        }
    }
}