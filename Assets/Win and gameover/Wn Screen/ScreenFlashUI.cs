using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlashUI : MonoBehaviour
{
    public Image overlay;
    public float flashInTime = 0.05f;
    public float flashOutTime = 0.12f;
    [Range(0f, 1f)] public float maxAlpha = 0.9f;

    void Awake()
    {
        if (!overlay) overlay = GetComponent<Image>();
        SetAlpha(0f);
    }

    public IEnumerator FlashOnce()
    {
        // Fade in fast
        float t = 0f;
        while (t < flashInTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(0f, maxAlpha, t / flashInTime));
            yield return null;
        }
        SetAlpha(maxAlpha);

        // Fade out
        t = 0f;
        while (t < flashOutTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(maxAlpha, 0f, t / flashOutTime));
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (!overlay) return;
        Color c = overlay.color;
        c.a = a;
        overlay.color = c;
    }
}