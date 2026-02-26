using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarMinimapPing : MonoBehaviour
{
    [Header("Refs")]
    public HeartManager heartManager;
    public Transform player;
    public Button pingButton;
    public Image cooldownFill;

    [Header("Radar UI")]
    public RectTransform radarArea;
    public RectTransform markerPrefab;

    [Header("Radar Settings")]
    public float radarRange = 15f;
    public float revealDuration = 2.5f;
    public float cooldownSeconds = 8f;

    private readonly List<RectTransform> markerPool = new();
    private bool onCooldown = false;

    private Coroutine pingCo;
    private Coroutine cooldownCo;

    void Start()
    {
        if (pingButton != null)
            pingButton.onClick.AddListener(TryPing);

        SetCooldownReady();
        HideAllMarkers();
    }

    void TryPing()
    {
        if (onCooldown) return;

        if (heartManager == null || player == null || radarArea == null || markerPrefab == null)
        {
            Debug.LogError("RadarMinimapPing: Missing references (HeartManager/Player/RadarArea/MarkerPrefab).");
            return;
        }

        // stop any previous ping routine (safety)
        if (pingCo != null) StopCoroutine(pingCo);
        pingCo = StartCoroutine(PingRoutine());
    }

    IEnumerator PingRoutine()
    {
        // show markers while reveal active
        float t = 0f;
        while (t < revealDuration)
        {
            t += Time.deltaTime;
            UpdateMarkers();
            yield return null;
        }

        HideAllMarkers();

        StartCooldown();
    }

    void StartCooldown()
    {
        // prevent 2 cooldown routines running at once
        if (cooldownCo != null) StopCoroutine(cooldownCo);
        cooldownCo = StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        if (pingButton != null) pingButton.interactable = false;

        // show cooldown overlay
        if (cooldownFill != null)
        {
            cooldownFill.gameObject.SetActive(true);
            cooldownFill.fillAmount = 1f; // start full
        }

        float t = 0f;
        while (t < cooldownSeconds)
        {
            t += Time.deltaTime; // if you use Time.timeScale freeze, change to Time.unscaledDeltaTime
            float n = Mathf.Clamp01(t / cooldownSeconds);

            // drain from 1 -> 0
            if (cooldownFill != null)
                cooldownFill.fillAmount = 1f - n;

            yield return null;
        }

        SetCooldownReady();
    }

    void SetCooldownReady()
    {
        onCooldown = false;
        if (pingButton != null) pingButton.interactable = true;

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 0f;
            cooldownFill.gameObject.SetActive(false); // ✅ disappears and stays gone
        }

        cooldownCo = null;
    }

    void UpdateMarkers()
    {
        List<Vector3> positions = heartManager.GetHeartPositions();
        if (positions == null) return;

        float radius = radarArea.rect.width * 0.5f;
        int visibleCount = 0;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2 offset = (Vector2)(positions[i] - player.position);
            float dist = offset.magnitude;

            if (dist > radarRange) continue;

            RectTransform marker = GetMarker(visibleCount);
            marker.gameObject.SetActive(true);

            Vector2 normalized = offset / radarRange; // -1..1
            Vector2 pos = normalized * radius;
            pos = ClampToCircle(pos, radius);

            marker.anchoredPosition = pos;
            visibleCount++;
        }

        for (int i = visibleCount; i < markerPool.Count; i++)
            markerPool[i].gameObject.SetActive(false);
    }

    RectTransform GetMarker(int index)
    {
        while (markerPool.Count <= index)
        {
            RectTransform m = Instantiate(markerPrefab, radarArea);
            markerPool.Add(m);
        }
        return markerPool[index];
    }

    void HideAllMarkers()
    {
        for (int i = 0; i < markerPool.Count; i++)
            markerPool[i].gameObject.SetActive(false);
    }

    Vector2 ClampToCircle(Vector2 pos, float radius)
    {
        float mag = pos.magnitude;
        if (mag > radius)
            return pos.normalized * radius;
        return pos;
    }
}