using System;
using Unity.Netcode;
using UnityEngine;

public class ButtonSwitch : NetworkBehaviour, IInteractable
{
    public event EventHandler OnInteractedWith;
    public event EventHandler OnButtonPressed;

    private const string DISPLAY_NAME = "Button";

    [SerializeField] private float pressCooldownTimer = 0.5f;

    private bool isOpen = true;
    private float currentPressCooldownTimer = 0;

    public void Interact(Interaction interaction)
    {
        if (currentPressCooldownTimer > 0f)
        {
            return;
        }

        ButtonPressServerRpc();

        OnInteractedWith?.Invoke(this, EventArgs.Empty);
    }

    private void ButtonPress()
    {
        if (Building.Singleton.HasPower())
        {
            isOpen = !isOpen;
        }

        currentPressCooldownTimer = pressCooldownTimer;

        OnButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (currentPressCooldownTimer > 0)
        {
            currentPressCooldownTimer -= Time.deltaTime;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ButtonPressServerRpc()
    {
        ButtonPressClientRpc();
    }

    [ClientRpc]
    private void ButtonPressClientRpc()
    {
        ButtonPress();
    }

    public string GetName()
    {
        return DISPLAY_NAME;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void SetIsOpen(bool isOpen)
    {
        this.isOpen = isOpen;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public bool IsInteractReady()
    {
        return true;
    }
}
