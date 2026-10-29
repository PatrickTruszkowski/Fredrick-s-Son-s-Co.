using System;
using Unity.Netcode;
using UnityEngine;

public class CholeraFreddy : NetworkBehaviour
{
    public event EventHandler OnFreddyShot;
    public event EventHandler OnNoseBoop;
    public event EventHandler OnWin;

    public static CholeraFreddy Singleton { get; private set; }

    [Header("References")]
    [SerializeField] private CholeraFreddyNose choleraFreddyNose = null;

    [Header("Product Spawning")]
    [SerializeField] private Transform winningProductPrefab = null;
    [SerializeField][Range(0, 100)] private int winningChancePercent = 5;

    private bool won = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        choleraFreddyNose.OnInteractedWith += CholeraFreddyNose_OnInteractedWith;
    }

    private void CholeraFreddyNose_OnInteractedWith(object sender, EventArgs e)
    {
        if (won)
        {
            NoseBoopServerRpc();

            return;
        }

        int randomNumber = UnityEngine.Random.Range(0, 101);

        if (randomNumber > winningChancePercent)
        {
            NoseBoopServerRpc();

            return;
        }

        WinServerRpc();

        MultiplayerManager.Singleton.SpawnProduct(winningProductPrefab, transform.position + transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    private void NoseBoopServerRpc()
    {
        NoseBoopClientRpc();
    }

    [ClientRpc]
    private void NoseBoopClientRpc()
    {
        OnNoseBoop?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WinServerRpc()
    {
        WinClientRpc();   
    }

    [ClientRpc]
    private void WinClientRpc()
    {
        won = true;

        OnWin?.Invoke(this, EventArgs.Empty);
    }

    public void ShootFreddy()
    {
        OnFreddyShot?.Invoke(this, EventArgs.Empty);
    }
}
