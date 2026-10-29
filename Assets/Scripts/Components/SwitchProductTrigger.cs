using System;
using UnityEngine;

public class SwitchProductTrigger : MonoBehaviour
{
    public event EventHandler OnProductSwitchTriggered;
    public event EventHandler OnProductSwitchFinished;

    public void TriggerSwitch()
    {
        OnProductSwitchTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerFinishSwitch()
    {
        OnProductSwitchFinished?.Invoke(this, EventArgs.Empty);
    }
}
