using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour
{
    public static Building Singleton {get; private set;}

    public event EventHandler OnBuildingPowerLost;

    [Header("References")]
    [SerializeField] private List<ButtonSwitch> buttonSwitchList;

    [Header("Settings")]
    [SerializeField] private float powerDrainMultiplier = 0.01f;

    private AudioSource audioSource;

    private float powerLevel = 1f;

    private void Awake()
    {
        Singleton = this;

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (powerLevel <= 0f)
        {
            return;
        }

        float powerUseage = 0;

        foreach (ButtonSwitch buttonSwitch in buttonSwitchList)
        {
            if (!buttonSwitch.IsOpen())
            {
                powerUseage += 1f / buttonSwitchList.Count;
            }
        }

        powerLevel -= powerUseage * powerDrainMultiplier * Time.deltaTime;
        powerLevel = Mathf.Clamp01(powerLevel);

        if (powerLevel <= 0f)
        {
            PowerLostClientRpc();
        }
    }

    public bool HasPower()
    {
        return powerLevel > 0f;
    }

    [ClientRpc]
    private void PowerLostClientRpc()
    {
        powerLevel = 0f;

        OnBuildingPowerLost?.Invoke(this, EventArgs.Empty);

        audioSource.PlayOneShot(audioSource.clip);

        foreach (ButtonSwitch buttonSwitch in buttonSwitchList)
        {
            buttonSwitch.SetIsOpen(true);
        }
    }
}
