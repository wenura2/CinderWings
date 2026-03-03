using UnityEngine;

public class TitleFadeIn : MonoBehaviour
{
    public float fadeDuration = 2f;
    public float scalePopAmount = 1.05f;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale * scalePopAmount, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }
}