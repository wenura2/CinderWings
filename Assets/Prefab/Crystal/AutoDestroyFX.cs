using UnityEngine;

public class AutoDestroyFX : MonoBehaviour
{
    public float lifeTime = 1f; // match your animation length

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}