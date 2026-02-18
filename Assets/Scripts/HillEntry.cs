using UnityEngine;

public class Hill : MonoBehaviour
{
   public Collider2D[] hillColliders;

   private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            foreach (Collider2D hill in hillColliders)
            {
                hill.enabled = false;
            }
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
    }
}
