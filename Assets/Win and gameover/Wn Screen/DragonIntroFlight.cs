using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DragonIntroFlight : MonoBehaviour
{
    [Header("Intro Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Flight Settings")]
    public float flySpeed = 5f;
    public float stopDistance = 0.05f;

    [Header("References")]
    public MonoBehaviour movementScript;   // drag your normal dragon movement script here
    public Collider2D[] dragonColliders;   // drag all colliders here (or leave empty to auto find)

    private Rigidbody2D rb;
    private bool introPlaying = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (dragonColliders == null || dragonColliders.Length == 0)
            dragonColliders = GetComponentsInChildren<Collider2D>();
    }

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        introPlaying = true;

        // Disable movement script
        if (movementScript != null)
            movementScript.enabled = false;

        // Disable colliders
        foreach (var col in dragonColliders)
            col.enabled = false;

        // Move dragon to Point A instantly (start position)
        transform.position = pointA.position;

        // Make Rigidbody kinematic for controlled intro movement
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Fly toward Point B
        while (Vector2.Distance(transform.position, pointB.position) > stopDistance)
        {
            Vector2 direction = (pointB.position - transform.position).normalized;
            rb.velocity = direction * flySpeed;

            yield return null;
        }

        // STOP EXACTLY
        rb.velocity = Vector2.zero;
        transform.position = pointB.position;

        // Restore physics
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Re-enable colliders
        foreach (var col in dragonColliders)
            col.enabled = true;

        // Re-enable movement script
        if (movementScript != null)
            movementScript.enabled = true;

        introPlaying = false;
    }
}