using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIEntranceBreathFromRight : MonoBehaviour
{
    [Header("Entrance (From Right)")]
    public float enterDuration = 0.9f;
    public float enterOvershoot = 25f;    // pixels (small overshoot looks nice)
    public float startOffsetX = 900f;     // positive = start from right (pixels)

    [Header("Breathing Loop")]
    public float breathScaleAmount = 0.03f; // 0.03 = 3%
    public float breathSpeed = 1.4f;        // bigger = faster
    public float wobbleAmount = 4f;         // pixels

    [Header("Start Delay (optional)")]
    public float startDelay = 0f;

    private RectTransform rt;
    private Vector2 targetAnchoredPos;
    private Vector3 targetScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        targetAnchoredPos = rt.anchoredPosition;
        targetScale = rt.localScale;

        // Start off-screen right
        rt.anchoredPosition = targetAnchoredPos + new Vector2(startOffsetX, 0f);
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(EnterThenBreath());
    }

    IEnumerator EnterThenBreath()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Slide in with a soft ease + tiny overshoot to the LEFT
        Vector2 startPos = rt.anchoredPosition;
        Vector2 overPos = targetAnchoredPos + new Vector2(-enterOvershoot, 0f);

        float t = 0f;
        while (t < enterDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / enterDuration);
            float eased = EaseOutCubic(p);
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, overPos, eased);
            yield return null;
        }

        // Settle back to target quickly
        float settleTime = 0.12f;
        t = 0f;
        Vector2 settleStart = rt.anchoredPosition;
        while (t < settleTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleTime);
            rt.anchoredPosition = Vector2.Lerp(settleStart, targetAnchoredPos, EaseOutCubic(p));
            yield return null;
        }

        // Breathing loop
        float seed = Random.Range(0f, 1000f);
        while (true)
        {
            float s = 1f + Mathf.Sin((Time.time + seed) * breathSpeed) * breathScaleAmount;
            rt.localScale = targetScale * s;

            float wobble = Mathf.Sin((Time.time + seed) * (breathSpeed * 1.2f)) * wobbleAmount;
            rt.anchoredPosition = targetAnchoredPos + new Vector2(wobble, 0f);

            yield return null;
        }
    }

    float EaseOutCubic(float x)
    {
        float a = 1f - x;
        return 1f - a * a * a;
    }
}