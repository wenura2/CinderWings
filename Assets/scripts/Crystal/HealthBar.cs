using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;   // assign the Fill child in Inspector

    [Header("Colors")]
    public Color highHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    private float maxWidth;

    private void Awake()
    {
        if (fillImage != null)
        {
            maxWidth = fillImage.rectTransform.sizeDelta.x;
        }
    }

    // Called by BreakableCrystal to update visuals
    public void UpdateHealthBar(float percent)
    {
        if (fillImage == null) return;

        // shrink width
        fillImage.rectTransform.sizeDelta = new Vector2(percent * maxWidth, fillImage.rectTransform.sizeDelta.y);

        // change color
        if (percent > 0.6f) fillImage.color = highHealthColor;
        else if (percent > 0.3f) fillImage.color = midHealthColor;
        else fillImage.color = lowHealthColor;
    }
}
