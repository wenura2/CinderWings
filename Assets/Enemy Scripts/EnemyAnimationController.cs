using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;
        anim.SetFloat("Speed", speed);
    }

    public void PlayAlert()
    {
        anim.SetTrigger("AlertTrigger");
    }

    public void SetAlerted(bool value)
    {
        anim.SetBool("IsAlerted", value);
    }
}
