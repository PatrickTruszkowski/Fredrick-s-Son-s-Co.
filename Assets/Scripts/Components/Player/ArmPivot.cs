using SpaceDoktor.Utils;
using Unity.Netcode;
using UnityEngine;

public class ArmPivot : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target = null;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        float sharpness = 24f;

        transform.forward = Smoothing.Exponential(transform.forward, target.forward, sharpness, Time.deltaTime);
    }
}
