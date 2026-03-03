using UnityEngine;
using System.Collections;
using Unity.Cinemachine;  // ✅ Cinemachine 3.x namespace

public class EggCameraShake : MonoBehaviour
{
    [Header("Assign your CinemachineCamera (CM 3.x)")]
    public CinemachineCamera cineCam;

    [Header("Optional: If you don't assign, it will auto-find on the same object")]
    public CinemachineBasicMultiChannelPerlin noise;

    private Coroutine routine;
    private float originalAmp;
    private float originalFreq;
    private bool cachedOriginal;

    void Awake()
    {
        if (cineCam == null)
            cineCam = GetComponent<CinemachineCamera>();

        if (noise == null)
            noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
            Debug.LogError("EggCameraShake: Add 'Cinemachine Basic Multi Channel Perlin' to the SAME GameObject as the CinemachineCamera.");

        CacheOriginal();
    }

    void OnEnable() => CacheOriginal();

    private void CacheOriginal()
    {
        if (noise == null) return;
        if (cachedOriginal) return;

        // CM3 renamed fields -> these are public fields in 3.1.x
        originalAmp = noise.AmplitudeGain;
        originalFreq = noise.FrequencyGain;
        cachedOriginal = true;
    }

    public void Shake(float duration, float amplitude, float frequency = 2f)
    {
        if (noise == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShakeRoutine(duration, amplitude, frequency));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency)
    {
        CacheOriginal();

        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        noise.AmplitudeGain = originalAmp;
        noise.FrequencyGain = originalFreq;

        routine = null;
    }
}