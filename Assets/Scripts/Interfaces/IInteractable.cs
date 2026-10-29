using System;
using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    public event EventHandler OnInteractedWith;

    public void Interact(Interaction interaction);

    public string GetName();

    public Transform GetTransform();

    public NetworkObject GetNetworkObject();

    public bool IsInteractReady();
}
