using UnityEngine;
using System.Collections;

public class LeafHideSpot : MonoBehaviour
{
    [Header("Healing In Bush")]
    public int healAmountPerTick = 1;
    public float healTickSeconds = 0.6f;

    [Header("Animator Params")]
    public string fullyHealedBool = "FullyHealed";

    [Header("Healing Effect (VFX)")]
    public GameObject healingEffectPrefab; // assign once in prefab
    public Vector3 effectOffset = new Vector3(0f, 0.6f, 0f); // above bush

    private Coroutine healRoutine;
    private GameObject spawnedEffect;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Animator playerAnimator = other.GetComponent<Animator>();
        EggHealth egg = other.GetComponent<EggHealth>();

        if (playerAnimator != null)
            playerAnimator.SetBool("Hidden", true);

        if (egg != null)
        {
            egg.isHidden = true;

            if (healRoutine != null)
                StopCoroutine(healRoutine);

            healRoutine = StartCoroutine(HealWhileInside(egg, playerAnimator));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Animator playerAnimator = other.GetComponent<Animator>();
        EggHealth egg = other.GetComponent<EggHealth>();

        if (playerAnimator != null)
            playerAnimator.SetBool("Hidden", false);

        if (egg != null)
            egg.isHidden = false;

        StopHealing();
    }

    private IEnumerator HealWhileInside(EggHealth egg, Animator playerAnimator)
    {
        yield return null;

        // Only show healing VFX if not already healed
        if (!egg.IsFullyHealed())
            StartHealingEffect();

        while (egg != null && egg.isHidden)
        {
            // Already healed -> stop effect and leave
            if (egg.IsFullyHealed())
            {
                if (playerAnimator != null)
                    playerAnimator.SetBool(fullyHealedBool, true);

                StopHealingEffect();
                yield break;
            }

            egg.Heal(healAmountPerTick);

            // After healing tick, if fully healed now:
            if (egg.IsFullyHealed())
            {
                if (playerAnimator != null)
                    playerAnimator.SetBool(fullyHealedBool, true);

                StopHealingEffect();
                yield break;
            }

            yield return new WaitForSeconds(healTickSeconds);
        }

        StopHealingEffect();
    }

    private void StopHealing()
    {
        if (healRoutine != null)
        {
            StopCoroutine(healRoutine);
            healRoutine = null;
        }

        StopHealingEffect();
    }

    private void StartHealingEffect()
    {
        if (healingEffectPrefab == null) return;
        if (spawnedEffect != null) return;

        Vector3 spawnPos = transform.position + effectOffset;

        spawnedEffect = Instantiate(healingEffectPrefab, spawnPos, Quaternion.identity);
        spawnedEffect.transform.SetParent(transform, true); // follow bush
    }

    private void StopHealingEffect()
    {
        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }
}
