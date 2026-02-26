using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EggAnimationController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer eggRenderer;

    [Header("Visual")]
    public float visualScale = 1f;

    [Header("Rolling and Tilt")]
    public float rollSpeedMultiplier = 120f;
    public float tiltBlend = 1f;

    [Header("Squash and Stretch")]
    public float stretchAmount = 0.18f;
    public float stretchSpeed = 8f;
    public float maxSpeed = 5f;

    [Header("Idle")]
    public float wobbleSpeed = 2f;
    public float wobbleAmount = 3f;

    [Header("Transition")]
    public float blendSpeed = 6f;
    public float fadeSpeed = 3f; // how fast to fade

    // runtime
    private Vector2 currentVelocity;
    private float rollAngle;
    private float idleBlend = 1f;
    private Vector3 baseScale;
    private Color baseColor;

    // fade
    private float fadeAlpha = 1f;
    [HideInInspector] public float targetAlpha = 1f;

    const float MinScaleMultiplier = 0.5f;
    const float MaxScaleMultiplier = 2.0f;

    void Start()
    {
        if (eggRenderer == null) eggRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale * Mathf.Max(0.0001f, visualScale);
        baseColor = eggRenderer.color;
    }

    void Update()
    {
        // Smoothly move fadeAlpha toward targetAlpha
        fadeAlpha = Mathf.MoveTowards(fadeAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool isMoving = currentVelocity.magnitude > 0.05f;
        float targetBlend = isMoving ? 0f : 1f;
        idleBlend = Mathf.MoveTowards(idleBlend, targetBlend, blendSpeed * Time.deltaTime);

        if (isMoving) AnimateRolling();
        else AnimateIdle();
    }

    public void SetVelocity(Vector2 velocity)
    {
        currentVelocity = velocity;
    }

    void AnimateIdle()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        transform.rotation = Quaternion.Euler(0f, 0f, wobble);

        // Apply fade alpha to base color only
        Color finalColor = baseColor;
        finalColor.a = fadeAlpha;
        eggRenderer.color = finalColor;

        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10f);
    }

    void AnimateRolling()
    {
        float speed = currentVelocity.magnitude;
        PrimaryDir primary = GetPrimaryDirection(currentVelocity);

        float moveAngle = 0f;
        switch (primary)
        {
            case PrimaryDir.Right: moveAngle = 0f; break;
            case PrimaryDir.Left:  moveAngle = 180f; break;
            case PrimaryDir.Up:    moveAngle = 90f; break;
            case PrimaryDir.Down:  moveAngle = -90f; break;
        }

        float signed = (primary == PrimaryDir.Left || primary == PrimaryDir.Down) ? -1f : 1f;
        rollAngle += speed * rollSpeedMultiplier * Time.deltaTime * signed;
        rollAngle = Mathf.Repeat(rollAngle, 360f);

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, moveAngle - rollAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(1f - idleBlend) * tiltBlend);

        float speedNorm = (maxSpeed > 0f) ? Mathf.Clamp01(speed / maxSpeed) : 1f;
        float osc = Mathf.Sin(Time.time * stretchSpeed) * stretchAmount * speedNorm;

        float scaleAlong = Mathf.Clamp(1f + osc, MinScaleMultiplier, MaxScaleMultiplier);
        float scalePerp  = Mathf.Clamp(1f - osc * 0.6f, MinScaleMultiplier, MaxScaleMultiplier);

        Vector3 targetScale = new Vector3(baseScale.x * scaleAlong, baseScale.y * scalePerp, baseScale.z);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Mathf.Clamp01(1f - idleBlend));

        // Apply fade alpha to base color while rolling
        Color finalColor = baseColor;
        finalColor.a = fadeAlpha;
        eggRenderer.color = finalColor;
    }

    enum PrimaryDir { None, Right, Left, Up, Down }

    PrimaryDir GetPrimaryDirection(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return PrimaryDir.None;
        return Mathf.Abs(v.x) >= Mathf.Abs(v.y) ? (v.x > 0f ? PrimaryDir.Right : PrimaryDir.Left)
                                                : (v.y > 0f ? PrimaryDir.Up : PrimaryDir.Down);
    }
}
