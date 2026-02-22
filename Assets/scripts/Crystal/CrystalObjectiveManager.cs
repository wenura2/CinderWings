using UnityEngine;
using UnityEngine.Events;

public class CrystalObjectiveManager : MonoBehaviour
{
    public static CrystalObjectiveManager Instance { get; private set; }

    [Header("Goal")]
    public int crystalsRequired = 3;

    [Header("Event (Door unlock)")]
    public UnityEvent onAllCrystalsBroken;

    private int brokenCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterCrystalBroken()
    {
        brokenCount++;

        if (brokenCount >= crystalsRequired)
            onAllCrystalsBroken?.Invoke();
    }
}