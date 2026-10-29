using System;
using System.Collections.Generic;
using UnityEngine;

public class Radio : Product, IUseable, IAlternateInteractable
{
    public event EventHandler OnUsed;
    public event EventHandler OnAlternateInteract;

    [SerializeField] private List<AudioClip> songList = null;

    private BatteryLife batteryLife = null;

    private int selectedSongIndex = 0;

    private bool isOn = false;

    private void Awake()
    {
        batteryLife = GetComponent<BatteryLife>();
    }

    public void Use()
    {
        if (isOn)
        {
            isOn = false;

            OnUsed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            isOn = true;

            OnUsed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AlternateInteract(Interaction interaction)
    {
        selectedSongIndex++;

        if (selectedSongIndex >= songList.Count)
        {
            selectedSongIndex = 0;
        }

        OnAlternateInteract?.Invoke(this, EventArgs.Empty);
    }

    public bool IsProductOn()
    {
        return isOn;
    }

    public BatteryLife GetBatteryLife()
    {
        return batteryLife;
    }

    public AudioClip GetCurrentSongAudioClip()
    {
        return songList[selectedSongIndex];
    }
}
