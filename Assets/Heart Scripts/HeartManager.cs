using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HeartManager : MonoBehaviour
{

    [Header("Win Sequence")]
    public EggWinSequenceSimple winSequence;
    [Header("Tilemaps (Spawn across all 3)")]
    public Tilemap level1Ground;
    public Tilemap level2Ground;
    public Tilemap level3Ground;

    [Header("Optional: Obstacle Tilemaps (Blocks spawning)")]
    public Tilemap level1Obstacles;
    public Tilemap level2Obstacles;
    public Tilemap level3Obstacles;

    [Header("Heart")]
    public GameObject heartPrefab;
    public int heartsToWin = 5;

    [Header("Spawn Rules")]
    public float minDistanceBetweenHearts = 6f;
    public int maxTotalAttempts = 2000;

    [Header("Optional: Collider Obstacles")]
    public LayerMask obstacleLayer;
    public float obstacleCheckRadius = 0.25f;

    [Header("Optional: Keep away from Player Spawn")]
    public Transform player;
    public float minDistanceFromPlayer = 4f;

    [Header("UI")]
    public HeartsUI heartsUI;

    private readonly List<Vector3> spawnedPositions = new();
    private int collected = 0;

    private struct MapPair
    {
        public Tilemap ground;
        public Tilemap obstacles;

        public MapPair(Tilemap g, Tilemap o)
        {
            ground = g;
            obstacles = o;
        }
    }

    void Start()
    {
        if (heartPrefab == null)
        {
            Debug.LogError("HeartManager: heartPrefab not assigned.");
            return;
        }

        var maps = BuildMapList();
        if (maps.Count == 0)
        {
            Debug.LogError("HeartManager: No ground tilemaps assigned.");
            return;
        }

        SpawnHeartsAcrossMaps(maps);

        if (heartsUI != null)
            heartsUI.SetHearts(0);
    }

    // ORIGINAL method (unchanged behavior)
    public void RegisterCollected()
    {
        collected++;

        if (heartsUI != null)
            heartsUI.SetHearts(collected);

        Debug.Log($"Hearts: {collected}/{heartsToWin}");

        if (collected >= heartsToWin)
            Win();
    }

    // NEW overload: remove from radar list then call original method
    public void RegisterCollected(Vector3 heartWorldPos)
    {
        const float removeDistance = 0.2f;

        int closestIndex = -1;
        float closestDist = float.MaxValue;

        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            float d = Vector2.Distance(heartWorldPos, spawnedPositions[i]);
            if (d < closestDist)
            {
                closestDist = d;
                closestIndex = i;
            }
        }

        if (closestIndex != -1 && closestDist <= removeDistance)
            spawnedPositions.RemoveAt(closestIndex);

        RegisterCollected(); // call the original increment/UI/win logic
    }

 private void Win()
{
    Debug.Log("✅ WIN! Collected all hearts!");

    if (winSequence != null)
        winSequence.Play();   // ✅ correct for EggWinSequenceSimple
    else
        Debug.LogError("HeartManager: winSequence not assigned!");
}
    private List<MapPair> BuildMapList()
    {
        var maps = new List<MapPair>(3);

        if (level1Ground != null) maps.Add(new MapPair(level1Ground, level1Obstacles));
        if (level2Ground != null) maps.Add(new MapPair(level2Ground, level2Obstacles));
        if (level3Ground != null) maps.Add(new MapPair(level3Ground, level3Obstacles));

        return maps;
    }

    private void SpawnHeartsAcrossMaps(List<MapPair> maps)
    {
        spawnedPositions.Clear();
        collected = 0;

        int spawned = 0;
        int attempts = 0;

        while (spawned < heartsToWin && attempts < maxTotalAttempts)
        {
            attempts++;

            MapPair map = maps[Random.Range(0, maps.Count)];

            if (!TryGetRandomValidPoint(map.ground, map.obstacles, out Vector3 pos))
                continue;

            if (!IsFarEnough(pos))
                continue;

            spawnedPositions.Add(pos);

            GameObject heart = Instantiate(heartPrefab, pos, Quaternion.identity);

            HeartPickup pickup = heart.GetComponent<HeartPickup>();
            if (pickup != null)
                pickup.manager = this;

            spawned++;
        }

        if (spawned < heartsToWin)
        {
            Debug.LogWarning($"HeartManager: Only spawned {spawned}/{heartsToWin}. " +
                             $"Lower minDistanceBetweenHearts or increase maxTotalAttempts.");
        }
    }

    private bool TryGetRandomValidPoint(Tilemap ground, Tilemap obstacles, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        BoundsInt bounds = ground.cellBounds;

        // A few tries per selected map
        for (int i = 0; i < 60; i++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);

            Vector3Int cell = new Vector3Int(x, y, 0);

            // must be on ground
            if (!ground.HasTile(cell))
                continue;

            // must not be obstacle tile (if tilemap provided)
            if (obstacles != null && obstacles.HasTile(cell))
                continue;

            Vector3 candidate = ground.GetCellCenterWorld(cell);

            // optional: keep away from player spawn
            if (player != null && Vector2.Distance(candidate, player.position) < minDistanceFromPlayer)
                continue;

            // optional: avoid collider obstacles
            if (obstacleLayer.value != 0)
            {
                if (Physics2D.OverlapCircle(candidate, obstacleCheckRadius, obstacleLayer) != null)
                    continue;
            }

            worldPos = candidate;
            return true;
        }

        return false;
    }

    private bool IsFarEnough(Vector3 candidate)
    {
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if (Vector2.Distance(candidate, spawnedPositions[i]) < minDistanceBetweenHearts)
                return false;
        }
        return true;
    }

    public List<Vector3> GetHeartPositions()
    {
        return spawnedPositions;
    }
}