using UnityEngine;

public class Phase2DoorTrigger : MonoBehaviour
{
    [Header("Door References")]
    public DoorOpen[] doors;   // ✅ array of doors instead of one

    private bool allKnightsDead = false;
    private bool doorsLockedBehind = false;

    private void Start()
    {
        if (Phase2ObjectiveManager.Instance != null)
        {
            Phase2ObjectiveManager.Instance.onAllBigKnightsDead.AddListener(OnAllBigKnightsDead);
            Debug.Log("Phase2DoorTrigger subscribed to Phase2ObjectiveManager");
        }
        else
        {
            Debug.LogError("Phase2ObjectiveManager not found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (Phase2ObjectiveManager.Instance != null)
        {
            Phase2ObjectiveManager.Instance.onAllBigKnightsDead.RemoveListener(OnAllBigKnightsDead);
        }
    }

    private void OnAllBigKnightsDead()
    {
        allKnightsDead = true;
        foreach (var d in doors)
        {
            if (d != null)
            {
                Debug.Log($"{d.name} unlocked → all BigKnights are dead");
                d.OpenDoor();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!allKnightsDead || doorsLockedBehind) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player crossed Phase 2 → closing all doors permanently");
            foreach (var d in doors)
            {
                if (d != null)
                    d.CloseDoor();
            }

            doorsLockedBehind = true; // ✅ Prevent reopening
        }
    }
}
