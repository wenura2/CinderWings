using System.Collections;
using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    public string walkBool = "IsWalking";
    public string attackTrigger = "Attack";
    public string hurtTrigger = "Hurt";
    public string deathTrigger = "Die";

    [Header("Death Fade")]
    public SpriteRenderer spriteRenderer;
    public float deathAnimWait = 1.2f;   // set to your death animation length
    public float fadeDuration = 1.5f;

    private bool dead;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetWalking(bool walking)
    {
        if (dead) return;
        if (animator && !string.IsNullOrWhiteSpace(walkBool))
            animator.SetBool(walkBool, walking);
    }

    public void PlayAttack()
    {
        if (dead) return;
        if (animator && !string.IsNullOrWhiteSpace(attackTrigger))
            animator.SetTrigger(attackTrigger);
    }

    public void PlayHurt()
    {
        if (dead) return;
        if (animator && !string.IsNullOrWhiteSpace(hurtTrigger))
            animator.SetTrigger(hurtTrigger);
    }

    public void PlayDeathAndFade()
    {
        if (dead) return;
        dead = true;

        if (animator)
        {
            if (!string.IsNullOrWhiteSpace(walkBool))
                animator.SetBool(walkBool, false);

            if (!string.IsNullOrWhiteSpace(deathTrigger))
                animator.SetTrigger(deathTrigger);
        }

        StartCoroutine(DeathFadeRoutine());
    }

    private IEnumerator DeathFadeRoutine()
    {
        // wait for death animation to finish (set this to match your clip length)
        if (deathAnimWait > 0f)
            yield return new WaitForSeconds(deathAnimWait);

        if (!spriteRenderer)
        {
            Destroy(gameObject);
            yield break;
        }

        float t = 0f;
        Color start = spriteRenderer.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, t / fadeDuration);
            spriteRenderer.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}