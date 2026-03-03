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

        Debug.Log("Opening door");

        if (doorCollider) doorCollider.enabled = false;

        if (doorRenderer != null)
            StartCoroutine(FadeOut());
        else
            gameObject.SetActive(false);
    }

    public void CloseDoor()
    {
        if (!opened) return;
        opened = false;

        Debug.Log("Closing door");

        if (doorRenderer != null)
            StartCoroutine(FadeIn());
        else
        {
            gameObject.SetActive(true);
            if (doorCollider) doorCollider.enabled = true;
        }
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

        doorRenderer.enabled = false;
    }

    private IEnumerator FadeIn()
    {
        doorRenderer.enabled = true;
        float t = 0f;

        // Start fully transparent
        doorRenderer.color = new Color(doorRenderer.color.r, doorRenderer.color.g, doorRenderer.color.b, 0f);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            doorRenderer.color = new Color(doorRenderer.color.r, doorRenderer.color.g, doorRenderer.color.b, a);
            yield return null;
        }

        if (doorCollider) doorCollider.enabled = true;
    }
}
