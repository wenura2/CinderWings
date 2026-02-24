using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Header("Heal Amount")]
    public int healAmount = 25;

    [Header("Optional FX")]
    public GameObject pickupFxPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.Heal(healAmount);
        }

        if (pickupFxPrefab != null)
            Instantiate(pickupFxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}