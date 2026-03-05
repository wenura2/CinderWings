using UnityEngine;
using UnityEngine.UI;

public class DragonHealthBar : MonoBehaviour
{
    [Header("References")]
    public DragHealth dragonHealth;   // link your DragHealth script
    public Slider healthSlider;       // link the UI Slider
    public Transform target;          // dragon transform
    public Vector3 offset = new Vector3(0, 2f, 0); // position above dragon

    private void Start()
    {
        if (dragonHealth != null && healthSlider != null)
        {
            healthSlider.maxValue = dragonHealth.MaxHealth;
            healthSlider.value = dragonHealth.CurrentHealth;
        }
    }

    private void Update()
    {
        if (dragonHealth != null && healthSlider != null)
        {
            healthSlider.value = dragonHealth.CurrentHealth;
        }

        // Keep bar above dragon and facing camera
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
