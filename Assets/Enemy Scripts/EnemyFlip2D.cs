using UnityEngine;

public class EnemyFlip2D : MonoBehaviour
{
    public SpriteRenderer sr;
    public Rigidbody2D rb;
    public float deadZone = 0.05f;

    void Awake()
    {
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (!sr || !rb) return;

        float x = rb.linearVelocity.x;
        if (Mathf.Abs(x) < deadZone) return;

        // If moving right -> face right (flip off), moving left -> face left (flip on)
        sr.flipX = x < 0f;
    }
}
