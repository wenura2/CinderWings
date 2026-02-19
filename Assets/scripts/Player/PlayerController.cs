using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Transform firePoint;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;

    private Vector3 lastMousePosition;

    private bool canAttack = true; // Controls one attack per click

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();

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
        HandleMovement();
        HandleAttackInput();
        FaceMouseOnlyIfMoved();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // =========================
    // Movement
    // =========================
    private void HandleMovement()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
        myAnimator.SetBool("isMoving", movement != Vector2.zero);
    }

    // =========================
    // Attack Input
    // =========================
    private void HandleAttackInput()
    {
        if (Mouse.current == null) return;

        // Right click triggers attack
        if (Mouse.current.rightButton.wasPressedThisFrame && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (firePrefab == null || firePoint == null)
        {
            Debug.LogWarning("FirePrefab or FirePoint not assigned!");
            return;
        }

        canAttack = false; // Prevent repeat until animation finishes

        // Trigger attack animation
        myAnimator.SetTrigger("Attack");
    }

    // =========================
    // Called via Animation Event
    // =========================
    public void SpawnFire()
    {
        if (firePrefab == null || firePoint == null)
            return;

        // Get mouse world position
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0f;

        // Calculate direction
        Vector2 direction = (mouseWorldPosition - firePoint.position).normalized;

        // Spawn fire projectile
        GameObject fire = Instantiate(firePrefab, firePoint.position, Quaternion.identity);

        // Send direction to projectile
        FireProjectile projectile = fire.GetComponent<FireProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }

        // Allow next attack after fire is spawned
        canAttack = true;
    }

    
    // Face Mouse
    

    private void FaceMouseOnlyIfMoved()
{
    if (Mouse.current == null) return;

    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    mouseWorldPosition.z = 0f;

    Vector3 scale = transform.localScale;

    if (mouseWorldPosition.x < transform.position.x)
        scale.x = Mathf.Abs(scale.x);   // face right
    else
        scale.x = -Mathf.Abs(scale.x);  // face left

    transform.localScale = scale;
}

    
}
