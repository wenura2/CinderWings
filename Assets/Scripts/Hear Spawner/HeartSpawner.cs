using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HeartSpawner : MonoBehaviour
{
    [Header("Ground Tilemaps (spawn ONLY where there is a tile)")]
    public Tilemap[] groundTilemaps;

    [Header("Obstacle Tilemaps (blocked areas - optional)")]
    public Tilemap[] obstacleTilemaps;

    [Header("Heart Prefab")]
    public GameObject heartPrefab;
    public int heartsToSpawn = 5;

    [Header("Placement Rules")]
    public float minDistanceBetweenHearts = 3f;
    public Transform player;
    public float minDistanceFromPlayer = 3f;

    [Header("Physics Block Check (optional but recommended)")]
    public LayerMask blockedLayer;          // trees, rocks, houses, etc.
    public float blockedCheckRadius = 0.25f;

    [Header("Attempts")]
    public int maxTotalAttempts = 2000;

    [Header("Spawn Pop Animation")]
    public float popDuration = 0.18f;
    public float popScale = 1.15f;

    private readonly List<Vector3> spawnedPositions = new();

    void Start()
    {
        SpawnHearts();
    }

    public void SpawnHearts()
    {
        if (heartPrefab == null || groundTilemaps == null || groundTilemaps.Length == 0)
        {
            Debug.LogError("HeartSpawner: Missing heartPrefab or groundTilemaps.");
            return;
        }

        spawnedPositions.Clear();

        int spawned = 0;
        int attempts = 0;

        while (spawned < heartsToSpawn && attempts < maxTotalAttempts)
        {
            attempts++;

            // Pick a random ground tilemap
            Tilemap ground = groundTilemaps[Random.Range(0, groundTilemaps.Length)];
            if (ground == null) continue;

            // Pick a random cell within bounds
            BoundsInt bounds = ground.cellBounds;
            Vector3Int cell = new Vector3Int(
                Random.Range(bounds.xMin, bounds.xMax),
                Random.Range(bounds.yMin, bounds.yMax),
                0
            );

            // Must have a ground tile at this cell
            if (!ground.HasTile(cell)) continue;

            // Must NOT be an obstacle tile (if obstacle tilemaps provided)
            if (IsBlockedByObstacleTile(cell)) continue;

            // Convert cell center to world position
            Vector3 worldPos = ground.GetCellCenterWorld(cell);
            worldPos.z = 0f;

            // Keep away from player
            if (player != null && Vector2.Distance(player.position, worldPos) < minDistanceFromPlayer)
                continue;

            // Keep hearts away from each other
            if (!IsFarFromOtherHearts(worldPos)) continue;

            // Physics check (avoid colliders/objects)
            if (blockedLayer.value != 0)
            {
                Collider2D hit = Physics2D.OverlapCircle(worldPos, blockedCheckRadius, blockedLayer);
                if (hit != null) continue;
            }

            // Spawn
            GameObject heart = Instantiate(heartPrefab, worldPos, Quaternion.identity);
            spawnedPositions.Add(worldPos);
            spawned++;

            // Pop animation
            StartCoroutine(PopRoutine(heart.transform));
        }

        if (spawned < heartsToSpawn)
        {
            Debug.LogWarning($"HeartSpawner: Spawned {spawned}/{heartsToSpawn}. Increase maxTotalAttempts or relax rules.");
        }
    }

    private bool IsBlockedByObstacleTile(Vector3Int cell)
    {
        if (obstacleTilemaps == null) return false;

        for (int i = 0; i < obstacleTilemaps.Length; i++)
        {
            Tilemap obs = obstacleTilemaps[i];
            if (obs == null) continue;

            // If obstacle tilemap has a tile at same cell, block it
            if (obs.HasTile(cell)) return true;
        }
        return false;
    }

    private bool IsFarFromOtherHearts(Vector3 worldPos)
    {
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if (Vector2.Distance(spawnedPositions[i], worldPos) < minDistanceBetweenHearts)
                return false;
        }
        return true;
    }

    private IEnumerator PopRoutine(Transform t)
    {
        if (t == null) yield break;

        Vector3 baseScale = t.localScale;
        t.localScale = baseScale * 0.01f;

        float half = popDuration * 0.5f;
        float time = 0f;

        // scale up
        while (time < half)
        {
            time += Time.deltaTime;
            float a = Mathf.Clamp01(time / half);
            t.localScale = Vector3.Lerp(baseScale * 0.01f, baseScale * popScale, a);
            yield return null;
        }

        // scale back to normal
        time = 0f;
        while (time < half)
        {
            time += Time.deltaTime;
            float a = Mathf.Clamp01(time / half);
            t.localScale = Vector3.Lerp(baseScale * popScale, baseScale, a);
            yield return null;
        }

        t.localScale = baseScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            Gizmos.DrawWireSphere(spawnedPositions[i], minDistanceBetweenHearts);
        }
    }
}