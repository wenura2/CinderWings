using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private PlayerHealth playerHealth;

    private Vector3 lastMousePosition;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        if (Mouse.current != null)
            lastMousePosition = Mouse.current.position.ReadValue();
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead()) return;

        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        // Block movement animation while attacking
        if (!isAttacking)
            myAnimator.SetBool("isMoving", movement != Vector2.zero);

        FaceMouseOnlyIfMoved();

        // Attack cooldown
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Left click to attack
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (attackTimer <= 0 && !isAttacking)
                StartCoroutine(AttackRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead()) return;

        // Stop moving while attacking
        if (!isAttacking)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        else
            rb.velocity = Vector2.zero;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        myAnimator.SetBool("isMoving", false);
        myAnimator.SetTrigger("Attack");

        // Wait for attack animation wind-up before dealing damage
        yield return new WaitForSeconds(0.2f);

        // Detect enemies in attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log("Hit enemy: " + hit.name);
            }
        }

        // Wait for rest of animation
        yield return new WaitForSeconds(attackCooldown - 0.2f);

        isAttacking = false;
    }

    private void FaceMouseOnlyIfMoved()
    {
        if (Mouse.current == null) return;

        Vector3 currentMousePosition = Mouse.current.position.ReadValue();

        if (currentMousePosition != lastMousePosition)
        {
            Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

            mySpriteRenderer.flipX = currentMousePosition.x > playerScreenPosition.x;

            lastMousePosition = currentMousePosition;
        }
    }

    // Visualize attack range in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}