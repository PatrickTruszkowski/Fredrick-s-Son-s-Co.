using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CameraSystem : NetworkBehaviour, IInteractable
{
    public event EventHandler OnInteractedWith;

    [Header("References")]
    [SerializeField] private RawImage cameraRenderImage = null;

    [Header("Camera List")]
    [SerializeField] private List<GameObject> cameraGameObjectList = null;

    private AudioSource audioSource = null;
    private int cameraIndex = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public string GetName()
    {
        return "Camera System " + "(" + cameraGameObjectList[cameraIndex].name + ")";
    }

    public void Interact(Interaction interaction)
    {
        if (interaction.IsZoomedIntoCameraSystem())
        {
            ChangeCameraServerRpc();
        }
    }

    private void ChangeCamera()
    {
        if (cameraIndex == cameraGameObjectList.Count - 1)
        {
            cameraGameObjectList[cameraIndex].SetActive(false);

            cameraIndex = 0;
        }
        else
        {
            cameraGameObjectList[cameraIndex].SetActive(false);

            cameraIndex++;
        }

        cameraGameObjectList[cameraIndex].SetActive(true);
        cameraRenderImage.texture = cameraGameObjectList[cameraIndex].GetComponent<Camera>().targetTexture;

        audioSource.pitch = UnityEngine.Random.Range(0.99f, 1.1f);
        audioSource.volume = UnityEngine.Random.Range(0.96f, 1f);

        audioSource.PlayOneShot(audioSource.clip);

        OnInteractedWith?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeCameraServerRpc()
    {
        ChangeCameraClientRpc();
    }

    [ClientRpc]
    private void ChangeCameraClientRpc()
    {
        ChangeCamera();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public bool IsInteractReady()
    {
        return false;
    }
}
