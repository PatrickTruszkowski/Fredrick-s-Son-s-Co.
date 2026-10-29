using System;
using Unity.Netcode;
using UnityEngine;

public class CholeraFreddyNose : MonoBehaviour, IInteractable
{
    public event EventHandler OnInteractedWith;

    private const string NAME = "Cholera Freddy";

    public string GetName()
    {
        return NAME;
    }

    public NetworkObject GetNetworkObject()
    {
        return null;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(Interaction interaction)
    {
        InteractServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc()
    {
        InteractClientRpc();
    }

    private void InteractClientRpc()
    {
        OnInteractedWith?.Invoke(this, EventArgs.Empty);
    }

    public bool IsInteractReady()
    {
        return true;
    }
}
