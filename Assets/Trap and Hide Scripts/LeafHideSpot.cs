using UnityEngine;

public class LeafHideSpot : MonoBehaviour
{
    private Animator playerAnimator;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered leaf trigger");
            playerAnimator = other.GetComponent<Animator>();

            if (playerAnimator != null)
                playerAnimator.SetBool("Hidden", true); // fade out
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited leaf trigger");

            if (playerAnimator != null)
                playerAnimator.SetBool("Hidden", false); // fade back in
        }
    }
}
