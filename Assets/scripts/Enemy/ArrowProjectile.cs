using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate arrow sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // Move arrow forward
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void Start()
    {
        // Destroy arrow after 3 seconds
        Destroy(gameObject, 3f);
    }
}
