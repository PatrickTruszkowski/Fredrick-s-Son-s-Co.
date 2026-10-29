using System;
using UnityEngine;

public class ThrowTrigger : MonoBehaviour
{
    public event EventHandler OnThrowTriggered;

    public void Throw()
    {
        OnThrowTriggered?.Invoke(this, EventArgs.Empty);
    }
}
