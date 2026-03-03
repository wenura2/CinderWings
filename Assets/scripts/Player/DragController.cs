using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack Range")]
    [SerializeField] private float attackRadius = 5f;   // how close enemies must be
    [SerializeField] private LayerMask enemyMask;       // which layers count as enemies

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;

    private bool canAttack = true; // Controls one attack per click

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

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

        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack)
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

        // Check for enemies in range
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyMask);
        if (enemiesInRange.Length == 0)
        {
            Debug.Log("No enemies in attack radius!");
            return; // cancel attack
        }

        canAttack = false; // Prevent repeat until animation finishes
        myAnimator.SetTrigger("Attack");
    }

    // =========================
    // Called via Animation Event
    // =========================
    public void SpawnFire()
    {
        if (firePrefab == null || firePoint == null)
            return;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0f;

        Vector2 direction = (mouseWorldPosition - firePoint.position).normalized;

        GameObject fire = Instantiate(firePrefab, firePoint.position, Quaternion.identity);

        FireProjectile projectile = fire.GetComponent<FireProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }

        canAttack = true;
    }

    // =========================
    // Face Mouse
    // =========================
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
