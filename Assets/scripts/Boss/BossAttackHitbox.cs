using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    public Boss boss;

    void Awake()
    {
        if (!boss) boss = GetComponentInParent<Boss>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (boss != null)
            boss.DamagePlayer(other);
    }
}