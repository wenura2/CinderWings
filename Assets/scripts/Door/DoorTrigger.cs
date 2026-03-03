using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door Reference")]
    public DoorDisappear door;

    [Header("Trigger Collider")]
    public BoxCollider2D triggerCollider;

    private bool objectivesComplete = false;

    private void Start()
    {
        if (CrystalObjectiveManager.Instance != null)
        {
            CrystalObjectiveManager.Instance.onAllObjectivesComplete.AddListener(OnObjectivesComplete);
            Debug.Log("DoorTrigger subscribed to CrystalObjectiveManager");
        }
        else
        {
            Debug.LogError("CrystalObjectiveManager not found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (CrystalObjectiveManager.Instance != null)
        {
            CrystalObjectiveManager.Instance.onAllObjectivesComplete.RemoveListener(OnObjectivesComplete);
        }
    }

    private void OnObjectivesComplete()
    {
        objectivesComplete = true;
        if (door != null)
        {
            Debug.Log("Door opened → all objectives complete");
            door.OpenDoor();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!objectivesComplete) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger → closing door");
            if (door != null)
                door.CloseDoor();
        }
    }
}
