using Unity.Netcode;

public class Battery : Product
{
    public void ChargeBatteryLife(BatteryLife batteryLife)
    {
        float minimumCharge = 0.9f;

        if (batteryLife.IsDead() || batteryLife.GetCharge() < minimumCharge)
        {
            ChargeBatteryLifeServerRpc(batteryLife.GetNetworkObject());

            MultiplayerManager.Singleton.DeleteProduct(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChargeBatteryLifeServerRpc(NetworkObjectReference batteryLifeNetworkObjectReference)
    {
        ChargeBatteryLifeClientRpc(batteryLifeNetworkObjectReference);
    }

    [ClientRpc]
    private void ChargeBatteryLifeClientRpc(NetworkObjectReference batteryLifeNetworkObjectReference)
    {
        batteryLifeNetworkObjectReference.TryGet(out NetworkObject batteryLifeNetworkObject);
        BatteryLife batteryLife = batteryLifeNetworkObject.GetComponent<BatteryLife>();

        batteryLife.Charge();
    }
}
