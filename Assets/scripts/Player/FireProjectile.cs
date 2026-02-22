using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 25;       // adjust damage in Inspector
    public float lifeTime = 3f;   // how long before projectile auto-destroys

    private Vector2 direction;

    // Called by the shooter to set projectile direction
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate projectile sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // Move projectile forward
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void Start()
    {
        // Destroy projectile after lifetime expires
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            // Pass both damage and projectile position for knockback
            enemy.TakeDamage(damage, transform.position);

            // Destroy projectile after hit
            Destroy(gameObject);
        }
    }
}
