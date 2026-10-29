using System;
using Unity.Netcode;
using UnityEngine;

public class Trash : MonoBehaviour, IInteractable
{
    public event EventHandler OnInteractedWith;

    public string GetName()
    {
        return "Trash";
    }

    public void Interact(Interaction interaction)
    {
        if (interaction.IsHoldingProduct())
        {
            Product product = interaction.GetHeldProduct();

            MultiplayerManager.Singleton.DeleteProduct(product);

            InteractServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc()
    {
        InteractClientRpc();
    }

    [ClientRpc]
    private void InteractClientRpc()
    {
        OnInteractedWith?.Invoke(this, EventArgs.Empty);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public NetworkObject GetNetworkObject()
    {
        return null;
    }

    public bool IsInteractReady()
    {
        return false;
    }
}
