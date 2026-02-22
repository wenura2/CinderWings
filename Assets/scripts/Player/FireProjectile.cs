using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 25;
    public float lifeTime = 3f;

    [Header("Impact")]
    public GameObject impactVfxPrefab;

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Called by shooter
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate sprite to face movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        rb.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Ignore self / player if needed
        if (collision.CompareTag("Player"))
            return;

        hasHit = true;

        // ===== ENEMY =====
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.position);
            Impact();
            return;
        }

        // ===== CRYSTAL =====
        BreakableCrystal crystal = collision.GetComponent<BreakableCrystal>();
        if (crystal != null)
        {
            crystal.TakeDamage(damage);
            Impact();
            return;
        }

        // ===== HIT WALL / ANYTHING ELSE =====
        Impact();
    }

    private void Impact()
    {
        if (impactVfxPrefab)
            Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}