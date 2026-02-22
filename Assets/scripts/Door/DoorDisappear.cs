using System.Collections;
using UnityEngine;

public class DoorDisappear : MonoBehaviour
{
    [Header("Door Parts")]
    public Collider2D doorCollider;       // blocking collider
    public SpriteRenderer doorRenderer;   // door sprite

    [Header("Fade")]
    public float fadeDuration = 0.6f;

    private bool opened = false;

    public void OpenDoor()
    {
        if (opened) return;
        opened = true;

        if (doorCollider) doorCollider.enabled = false;

        if (doorRenderer != null)
            StartCoroutine(FadeOut());
        else
            gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color start = doorRenderer.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            doorRenderer.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}