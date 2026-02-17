using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class EggMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private EggAnimationController animController;
    private EggControls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        animController = GetComponent<EggAnimationController>();
        controls = new EggControls();

        // keep animator aware of max speed for normalization
        if (animController != null)
            animController.maxSpeed = moveSpeed;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // Read input in Update for responsiveness
        inputDirection = controls.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Apply physics velocity
        rb.velocity = inputDirection * moveSpeed;

        // Send the actual physics velocity to the animation controller (physics-accurate)
        if (animController != null)
            animController.SetVelocity(rb.velocity);
    }
}
