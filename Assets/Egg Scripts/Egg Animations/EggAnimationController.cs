using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EggAnimationController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer eggRenderer;

    [Header("Rolling / Tilt")]
    public float rollSpeedMultiplier = 250f; // visual roll per unit speed
    public float tiltBlend = 1f;             // how strongly rotation aligns to movement direction

    [Header("Squash & Stretch")]
    public float stretchAmount = 0.12f;      // max exaggeration
    public float stretchSpeed = 8f;          // oscillation speed
    [Tooltip("Set to EggMovement.moveSpeed or a sensible max for normalization")]
    public float maxSpeed = 5f;

    [Header("Idle")]
    public float wobbleSpeed = 2f;
    public float wobbleAmount = 3f;
    public Color glowColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseStrength = 0.4f;

    [Header("Transition")]
    public float blendSpeed = 6f;

    // runtime
    private Vector2 currentVelocity;
    private float rollAngle;
    private float idleBlend = 1f; // 1 = idle, 0 = rolling
    private Vector3 baseScale;
    private Color baseColor;

    void Start()
    {
        if (eggRenderer == null) eggRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = eggRenderer.color;
    }

    void Update()
    {
        bool isMoving = currentVelocity.magnitude > 0.05f;
        float targetBlend = isMoving ? 0f : 1f;
        idleBlend = Mathf.MoveTowards(idleBlend, targetBlend, blendSpeed * Time.deltaTime);

        if (isMoving) AnimateRolling();
        else AnimateIdle();
    }

    // Called from EggMovement.FixedUpdate
    public void SetVelocity(Vector2 velocity)
    {
        currentVelocity = velocity;
    }

    // --------------------
    // IDLE
    // --------------------
    void AnimateIdle()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        transform.rotation = Quaternion.Euler(0f, 0f, wobble);

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * pulseStrength;
        eggRenderer.color = Color.Lerp(baseColor, glowColor, pulse);

        transform.localScale = baseScale;
    }

    // --------------------
    // ROLLING (Left uses Down animation)
    // --------------------
    void AnimateRolling()
    {
        float speed = currentVelocity.magnitude;

        // 1) Determine primary direction (dominant axis)
        PrimaryDir primary = GetPrimaryDirection(currentVelocity);

        // 2) Map Left -> Down so left uses the exact same animation as downward movement
        if (primary == PrimaryDir.Left)
            primary = PrimaryDir.Down;

        // 3) Compute moveAngle for mapped primary direction
        float moveAngle = 0f;
        switch (primary)
        {
            case PrimaryDir.Right: moveAngle = 0f; break;
            case PrimaryDir.Up:    moveAngle = 90f; break;
            case PrimaryDir.Down:  moveAngle = -90f; break; // downward orientation
            default:               moveAngle = 0f; break;
        }

        // 4) Determine roll sign so spin matches mapped direction (use same sign as Down)
        float signed = 1f;
        if (primary == PrimaryDir.Down) signed = -1f;

        // 5) Accumulate rollAngle using signed speed
        rollAngle += speed * rollSpeedMultiplier * Time.deltaTime * signed;

        // 6) Combine orientation: align to mapped movement direction, then apply rolling rotation
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, moveAngle - rollAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(1f - idleBlend) * tiltBlend);

        // 7) Squash & stretch along movement axis (preserve perceived volume)
        float speedNorm = (maxSpeed > 0f) ? Mathf.Clamp01(speed / maxSpeed) : 1f;
        float osc = Mathf.Sin(Time.time * stretchSpeed) * stretchAmount * speedNorm;

        // For Down (and mapped Left) we want the same axis behavior as Down:
        // scaleAlong is along the movement axis (local X after rotation)
        float scaleAlong = 1f + osc;
        float scalePerp  = 1f - osc * 0.6f;

        scaleAlong = Mathf.Max(0.5f, scaleAlong);
        scalePerp  = Mathf.Max(0.5f, scalePerp);

        Vector3 targetScale = new Vector3(baseScale.x * scaleAlong, baseScale.y * scalePerp, baseScale.z);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Mathf.Clamp01(1f - idleBlend));

        // 8) Color: no glow while rolling
        eggRenderer.color = Color.Lerp(eggRenderer.color, baseColor, Time.deltaTime * 10f);
    }

    // Helper: primary axis direction
    enum PrimaryDir { None, Right, Left, Up, Down }

    PrimaryDir GetPrimaryDirection(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return PrimaryDir.None;

        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);

        if (ax >= ay)
            return v.x > 0f ? PrimaryDir.Right : PrimaryDir.Left;
        else
            return v.y > 0f ? PrimaryDir.Up : PrimaryDir.Down;
    }
}
