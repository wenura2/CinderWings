using UnityEngine;
using UnityEngine.UI;

public class CrystalHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;   // assign the Fill child in Inspector

    [Header("Colors")]
    public Color highHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Animation")]
    public float lerpSpeed = 8f; // higher = faster bar update

    private float targetFill = 1f; // desired fill amount
    private float currentFill = 1f; // actual displayed fill

    private void Awake()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f; // start full
        }
    }

    private void Update()
    {
        if (fillImage == null) return;

        // Smoothly interpolate current fill toward target fill
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * lerpSpeed);
        fillImage.fillAmount = currentFill;
    }

    /// <summary>
    /// Updates the health bar target fill and color.
    /// </summary>
    public void UpdateHealthBar(float percent)
    {
        if (fillImage == null) return;

        targetFill = Mathf.Clamp01(percent);

        if (percent > 0.6f)
            fillImage.color = highHealthColor;
        else if (percent > 0.3f)
            fillImage.color = midHealthColor;
        else
            fillImage.color = lowHealthColor;
    }

    /// <summary>
    /// Destroys the entire health bar prefab (Canvas + Background + Fill).
    /// </summary>
    public void DestroyBar()
    {
        Destroy(gameObject); // destroys the Canvas root and all children
    }
}
