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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.drag = deceleration;
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
        inputDirection = controls.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = inputDirection * moveSpeed;
        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (rb.velocity.magnitude > moveSpeed)
            rb.velocity = rb.velocity.normalized * moveSpeed;

        if (animController != null)
            animController.SetVelocity(rb.velocity);
    }
}
