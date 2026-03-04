using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineShakeM2 : MonoBehaviour
{
    private CinemachineCamera cmCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();

        noise = cmCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise)
                as CinemachineBasicMultiChannelPerlin;

        if (noise == null)
        {
            Debug.LogError("Noise component not found. Make sure Noise = Basic Multi Channel Perlin.");
        }
    }

    public void Shake(float amplitude, float duration)
    {
        if (noise == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(amplitude, duration));
    }

    private IEnumerator ShakeRoutine(float amplitude, float duration)
    {
        noise.AmplitudeGain = amplitude;

        yield return new WaitForSeconds(duration);

        noise.AmplitudeGain = 0f;
        shakeRoutine = null;
    }
}