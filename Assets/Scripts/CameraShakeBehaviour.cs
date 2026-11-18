using UnityEngine;
using Cinemachine;

/// <summary>
/// Put on the Cinemachine virtual camera (CM vcam1).
/// Needs Noise = Basic Multi Channel Perlin on that vcam.
/// Shake() turns the Perlin amplitude up for a short time, then back to 0.
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShakeBehaviour : MonoBehaviour
{
    public float shakeIntensity = 3.0f;     // Perlin amplitude gain while shaking
    public float shakeDuration = 0.3f;      // seconds

    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;

    void Start()
    {
        CinemachineVirtualCamera vcam = GetComponent<CinemachineVirtualCamera>();
        perlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin == null)
        {
            Debug.LogWarning("CameraShake: set Noise to 'Basic Multi Channel Perlin' on the vcam.");
            return;
        }
        perlin.m_AmplitudeGain = 0.0f;      // no shake at the start
    }

    public void Shake()
    {
        if (perlin == null) return;
        perlin.m_AmplitudeGain = shakeIntensity;
        shakeTimer = shakeDuration;
    }

    void Update()
    {
        if (shakeTimer > 0.0f)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0.0f)
            {
                perlin.m_AmplitudeGain = 0.0f;   // stop shaking
            }
        }
    }
}
