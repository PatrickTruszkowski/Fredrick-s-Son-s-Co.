using System;
using UnityEngine;

public class BotFootsteps : MonoBehaviour
{
    public event EventHandler OnFootstep;

    public void Footstep()
    {
        OnFootstep?.Invoke(this, EventArgs.Empty);
    }
}
