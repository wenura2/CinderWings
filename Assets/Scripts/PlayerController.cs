using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;

    private Vector3 lastMousePosition;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();

        // Store initial mouse position
        if (Mouse.current != null)
            lastMousePosition = Mouse.current.position.ReadValue();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        // myAnimator.SetFloat("moveX", movement.x);
        // myAnimator.SetFloat("moveY", movement.y);
        myAnimator.SetBool("isMoving", movement != Vector2.zero);

        FaceMouseOnlyIfMoved();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void FaceMouseOnlyIfMoved()
    {
        if (Mouse.current == null) return;

        Vector3 currentMousePosition = Mouse.current.position.ReadValue();

        // Only update facing if mouse actually moved
        if (currentMousePosition != lastMousePosition)
        {
            Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

            if (currentMousePosition.x > playerScreenPosition.x)
                mySpriteRenderer.flipX = true;
            else
                mySpriteRenderer.flipX = false;

            lastMousePosition = currentMousePosition;
        }
    }
}