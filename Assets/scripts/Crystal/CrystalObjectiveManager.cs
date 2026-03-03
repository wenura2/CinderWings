using UnityEngine;
using UnityEngine.Events;

public class CrystalObjectiveManager : MonoBehaviour
{
    public static CrystalObjectiveManager Instance { get; private set; }

    [Header("Crystal Goal")]
    [Tooltip("How many crystals must be broken to progress.")]
    public int crystalsRequired = 3;

    [Header("Enemy Goal")]
    [Tooltip("How many enemies must be killed to progress.")]
    public int enemiesRequired = 38;

    [Header("Event (Door unlock)")]
    public UnityEvent onAllObjectivesComplete;

    [Header("Runtime Progress (debug only)")]
    [SerializeField] private int brokenCount = 0;
    [SerializeField] private int enemiesKilled = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("CrystalObjectiveManager Awake → Instance set");
    }

    public void RegisterCrystalBroken()
    {
        brokenCount++;
        Debug.Log($"Crystal broken! Count = {brokenCount}/{crystalsRequired}");
        CheckObjectives();
    }

    public void RegisterEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"Enemy killed! Count = {enemiesKilled}/{enemiesRequired}");
        CheckObjectives();
    }

    private void CheckObjectives()
    {
        if (brokenCount >= crystalsRequired && enemiesKilled >= enemiesRequired)
        {
            Debug.Log("All objectives complete → unlocking door");
            onAllObjectivesComplete?.Invoke();
        }
    }
}
