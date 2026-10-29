using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Singleton;

    [SerializeField] private GameObject menuCameraGameObject = null;
    [SerializeField] private RenderTexture[] playerRenderTextureArray = null;
    [SerializeField] private List<Transform> productPrefabList = null;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        menuCameraGameObject.SetActive(false);
    }

    public RenderTexture[] GetPlayerRenderTextureArray()
    {
        return playerRenderTextureArray;
    }

    public void SpawnProduct(Transform prefab, Vector3 position)
    {
        int prefabIndex = productPrefabList.IndexOf(prefab);

        SpawnProductServerRpc(prefabIndex, position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProductServerRpc(int prefabIndex, Vector3 position)
    {
        Transform productTransform = Instantiate(productPrefabList[prefabIndex]);

        NetworkObject productNetworkObject = productTransform.GetComponent<NetworkObject>();
        productNetworkObject.Spawn(true);

        SetProductPositionClientRpc(productNetworkObject, position);
    }

    [ClientRpc]
    private void SetProductPositionClientRpc(NetworkObjectReference productNetworkObjectReference, Vector3 position)
    {
        productNetworkObjectReference.TryGet(out NetworkObject productNetworkObject);

        productNetworkObject.transform.position = position;
    }

    public void DeleteProduct(Product product)
    {
        DeleteProductServerRpc(product.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteProductServerRpc(NetworkObjectReference productNetworkObjectReference)
    {
        productNetworkObjectReference.TryGet(out NetworkObject productNetworkObject);
        Product product = productNetworkObject.GetComponent<Product>();

        product.Delete();
    }
}
