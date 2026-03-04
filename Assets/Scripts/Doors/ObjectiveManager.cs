using UnityEngine;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Mission Goals")]
    [Tooltip("How many buildings must be destroyed to progress.")]
    public int buildingsRequired = 3;

    [Tooltip("How many enemies must be killed to progress.")]
    public int enemiesRequired = 10;

    [Header("Event (Door unlock)")]
    public UnityEvent onAllObjectivesComplete;

    [Header("Runtime Progress (debug only)")]
    [SerializeField] private int buildingsDestroyed = 0;
    [SerializeField] private int enemiesKilled = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterBuildingDestroyed()
    {
        buildingsDestroyed++;
        Debug.Log($"Building destroyed! Count = {buildingsDestroyed}/{buildingsRequired}");
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
        if (buildingsDestroyed >= buildingsRequired && enemiesKilled >= enemiesRequired)
        {
            Debug.Log("All objectives complete → unlocking door");
            onAllObjectivesComplete?.Invoke();
        }
    }
}