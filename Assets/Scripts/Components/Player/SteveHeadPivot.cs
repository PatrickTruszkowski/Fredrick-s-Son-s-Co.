using Unity.Netcode;
using UnityEngine;

public class SteveHeadPivot : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;

    private void LateUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        transform.position = playerCameraTransform.position;
        transform.localRotation = playerCameraTransform.localRotation;
    }
}
