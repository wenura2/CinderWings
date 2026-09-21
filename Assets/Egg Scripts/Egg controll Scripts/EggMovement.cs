using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class EggMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 40f;
    public float deceleration = 0.8f;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private EggAnimationController animController;
    private EggControls controls;

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.linearDamping = deceleration;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        animController = GetComponent<EggAnimationController>();
        controls = new EggControls();

        if (animController != null)
            animController.maxSpeed = moveSpeed;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (isDead)
        {
            inputDirection = Vector2.zero;
            return;
        }

        inputDirection = controls.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 targetVelocity = inputDirection * moveSpeed;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (rb.linearVelocity.magnitude > moveSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;

        if (animController != null)
            animController.SetVelocity(rb.linearVelocity);
    }

    // ✅ Call this when the egg dies
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        inputDirection = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // ✅ hard stop: no physics movement possible
        }

        if (animController != null)
            animController.SetVelocity(Vector2.zero);
    }
}
