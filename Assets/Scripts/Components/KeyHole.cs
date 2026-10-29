using System;
using Unity.Netcode;
using UnityEngine;

public class KeyHole : MonoBehaviour, IInteractable
{
    public event EventHandler OnInteractedWith;

    private const string INTERACTABLE_NAME = "Key Hole";

    public void Interact(Interaction interaction)
    {
        if (interaction.IsHoldingProduct() && interaction.GetHeldProduct().TryGetKey(out Key key))
        {
            key.Delete();
            Debug.Log("Case Opened!");

            OnInteractedWith?.Invoke(this, EventArgs.Empty);
        }
    }

    public string GetName()
    {
        return INTERACTABLE_NAME;
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
