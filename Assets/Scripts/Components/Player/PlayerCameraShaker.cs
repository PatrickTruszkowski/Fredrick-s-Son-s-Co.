using Unity.Netcode;
using UnityEngine;

public enum ShakeIntensity
{
    Soft, Moderate, Heavy, Extreme
}

public class PlayerCameraShaker : NetworkBehaviour
{
    public static PlayerCameraShaker LocalSingleton { get; private set; }

    private const float CAMERA_ZERO_OUT_SPEED = 16f;
    private const float SOFT_SPEED = 10f;
    private const float MODERATE_SPEED = 25f;
    private const float HEAVY_SPEED = 50f;
    private const float EXTREME_SPEED = 100f;
    private const float SOFT_DISTANCE = 8f;
    private const float MODERATE_DISTANCE = 12f;
    private const float HEAVY_DISTANCE = 16f;
    private const float EXTREME_DISTANCE = 20f;

    private Vector3 shakeOffset = Vector3.zero;
    private float shakeDuration = 0f;
    private float shakeTime = 0f;
    private float shakeSpeed = 5;
    private bool shake = false;
    private Vector3 currentShakeOrigin;
    private float currentMinimumDistance;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalSingleton = this;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!shake)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, CAMERA_ZERO_OUT_SPEED * Time.deltaTime);

            return;
        }

        if (shakeTime < shakeDuration)
        {
            shakeTime += Time.deltaTime;

            shakeOffset = Vector3.zero;

            shakeOffset.x = Random.Range(-0.2f, 0.2f);
            shakeOffset.y = Random.Range(-0.2f, 0.2f);

            float distance = Vector3.Distance(transform.position, currentShakeOrigin);
            float shakeSpeedMultiplier = 1f - Mathf.InverseLerp(0f, currentMinimumDistance, distance);

            shakeSpeed = Mathf.Lerp(shakeSpeed, 0f, shakeTime / shakeDuration * 20f * Time.deltaTime);

            shakeOffset.x *= shakeSpeedMultiplier;
            shakeOffset.y *= shakeSpeedMultiplier;

            transform.localPosition = Vector3.Lerp(transform.localPosition, shakeOffset, shakeSpeed * Time.deltaTime);
        }
        else
        {
            shake = false;
            shakeDuration = 0f;
            shakeTime = 0f;
            shakeSpeed = 0f;
        }
    }

    public void Shake(Vector3 shakeOrigin, ShakeIntensity shakeIntensity = ShakeIntensity.Moderate, float duration = 1f)
    {
        switch (shakeIntensity)
        {
            case ShakeIntensity.Soft:

                shakeSpeed += SOFT_SPEED;
                currentMinimumDistance = SOFT_DISTANCE;

                break;
            case ShakeIntensity.Moderate:

                shakeSpeed += MODERATE_SPEED;
                currentMinimumDistance = MODERATE_DISTANCE;

                break;
            case ShakeIntensity.Heavy:

                shakeSpeed += HEAVY_SPEED;
                currentMinimumDistance = HEAVY_DISTANCE;

                break;
            case ShakeIntensity.Extreme:

                shakeSpeed += EXTREME_SPEED;
                currentMinimumDistance = EXTREME_DISTANCE;

                break;
        }

        currentShakeOrigin = shakeOrigin;
        shakeDuration = duration;
        shakeTime = 0f;
        shake = true;
    }
}
