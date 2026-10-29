using System;

public class Flashlight : Product, IUseable
{
    public event EventHandler OnUsed;

    private BatteryLife batteryLife = null;

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

    public bool IsProductOn()
    {
        return isOn;
    }

    public BatteryLife GetBatteryLife()
    {
        return batteryLife;
    }
}
