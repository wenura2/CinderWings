using UnityEngine;

public class LeafHideSpot : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered leaf trigger");

            Animator playerAnimator = other.GetComponent<Animator>();
            EggHealth egg = other.GetComponent<EggHealth>();

            if (playerAnimator != null)
                playerAnimator.SetBool("Hidden", true);

            if (egg != null)
                egg.isHidden = true;  // 🔥 Important
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited leaf trigger");

            Animator playerAnimator = other.GetComponent<Animator>();
            EggHealth egg = other.GetComponent<EggHealth>();

            if (playerAnimator != null)
                playerAnimator.SetBool("Hidden", false);

            if (egg != null)
                egg.isHidden = false;  // 🔥 Important
        }
    }
}
