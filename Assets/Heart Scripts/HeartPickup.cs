using UnityEngine;
using System.Collections;

public class HeartPickup : MonoBehaviour
{
    [HideInInspector] public HeartManager manager;

    [Header("Effect Settings")]
    public Color effectColor = Color.yellow;
    public float effectDuration = 0.5f;
    public float maxScale = 1.8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Create effect from code
        StartCoroutine(PlayCollectEffect());

        if (manager != null)
        manager.RegisterCollected(transform.position);

        // Hide heart immediately
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, effectDuration);
    }

    private IEnumerator PlayCollectEffect()
    {
        // Create temporary object
        GameObject effect = new GameObject("HeartCollectEffect");
        effect.transform.position = transform.position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
        sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;

        sr.color = effectColor;

        float time = 0f;

        while (time < effectDuration)
        {
            time += Time.deltaTime;

            float t = time / effectDuration;

            // Scale up
            float scale = Mathf.Lerp(1f, maxScale, t);
            effect.transform.localScale = Vector3.one * scale;

            // Fade out
            sr.color = new Color(effectColor.r, effectColor.g, effectColor.b, 1f - t);

            yield return null;
        }

        Destroy(effect);
    }
}
