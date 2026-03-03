using UnityEngine;

public class DoorsTrigger : MonoBehaviour
{
    [Header("Door Reference")]
    public DoorOpen door;

    private bool objectivesComplete = false;

    private void Start()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.onAllObjectivesComplete.AddListener(OnObjectivesComplete);
            Debug.Log("DoorsTrigger subscribed to ObjectiveManager");
        }
        else
        {
            Debug.LogError("ObjectiveManager not found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.onAllObjectivesComplete.RemoveListener(OnObjectivesComplete);
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
