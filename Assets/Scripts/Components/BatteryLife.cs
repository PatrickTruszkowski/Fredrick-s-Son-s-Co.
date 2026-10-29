using System;
using Unity.Netcode;
using UnityEngine;

public class BatteryLife : NetworkBehaviour
{
    public event EventHandler OnBatteryLifeDied;
    public event EventHandler OnBatteryLifeCharged;

    [Header("Battery Settings")]
    [SerializeField] private float dischargeAmountPerSecond = 0.01f;

    [Header("Debug")]
    [SerializeField] private bool isTesting = false;

    private IUseable useableProduct = null;

    private float charge = 1f;

    private bool isDead = false;

    private void Awake()
    {
        useableProduct = GetComponent<IUseable>();
    }

    private void Update()
    {
        Discharge();
    }

    private void Discharge()
    {
        if (isTesting) return;

        isDead = charge <= 0f;

        if (isDead)
        {
            OnBatteryLifeDied?.Invoke(this, EventArgs.Empty);

            return;
        }

        if (useableProduct.IsProductOn() && !isDead)
        {
            charge -= Time.deltaTime * dischargeAmountPerSecond;
            charge = Mathf.Clamp(charge, 0f, 1f);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetCharge()
    {
        return charge;
    }

    public void Charge()
    {
        charge = 1f;

        OnBatteryLifeCharged?.Invoke(this, EventArgs.Empty);
    }

    public IUseable GetUseableProduct()
    {
        return useableProduct;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
