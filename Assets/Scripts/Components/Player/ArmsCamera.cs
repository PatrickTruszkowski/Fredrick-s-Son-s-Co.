using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ArmsCamera : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    private Camera armsCamera;

    private void Awake()
    {
        armsCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        armsCamera.fieldOfView = playerCamera.fieldOfView;
    }
}
