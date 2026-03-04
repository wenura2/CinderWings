using UnityEngine;
using UnityEngine.Events;

public class Phase2ObjectiveManager : MonoBehaviour
{
    public static Phase2ObjectiveManager Instance { get; private set; }

    [Header("Phase 2 Goal")]
    [Tooltip("How many BigKnights must be killed to open the Phase 2 door.")]
    public int bigKnightsRequired = 3;

    private int bigKnightsKilled = 0;

    [Header("Events")]
    public UnityEvent onAllBigKnightsDead;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterBigKnightKilled()
    {
        bigKnightsKilled++;
        Debug.Log($"BigKnight killed! Count = {bigKnightsKilled}/{bigKnightsRequired}");

        if (bigKnightsKilled >= bigKnightsRequired)
        {
            Debug.Log("All BigKnights defeated → Phase 2 door unlocks!");
            onAllBigKnightsDead?.Invoke();
        }
    }
}