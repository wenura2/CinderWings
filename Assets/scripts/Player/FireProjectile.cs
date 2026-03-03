using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FireProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 25;
    public float lifeTime = 3f;

    [Header("Collision")]
    public string ignoreTag = "Player";         // projectile won't hit this tag
    public LayerMask hitMask = ~0;              // what layers this projectile can hit (default: everything)

    [Header("Impact")]
    public GameObject impactVfxPrefab;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 direction;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // This script uses triggers
        col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Called by shooter
    public void SetDirection(Vector2 dir)
    {
        direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        // Rotate sprite to face movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Ignore tag (player)
        if (!string.IsNullOrEmpty(ignoreTag) && other.CompareTag(ignoreTag))
            return;

        // Layer mask filter
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        hasHit = true;

        // ===== BOSS =====
        Boss boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Impact();
            return;
        }

        // ===== ENEMY =====
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            // Keep your existing signature
            enemy.TakeDamage(damage, transform.position);
            Impact();
            return;
        }

        // ===== CRYSTAL =====
        BreakableCrystal crystal = other.GetComponent<BreakableCrystal>();
        if (crystal != null)
        {
            crystal.TakeDamage(damage);
            Impact();
            return;
        }

        // ===== HIT ANYTHING ELSE =====
        Impact();
    }

    private void Impact()
    {
        if (impactVfxPrefab)
            Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}