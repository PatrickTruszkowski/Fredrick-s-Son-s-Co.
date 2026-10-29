using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Product : NetworkBehaviour, IInteractable
{
    public event EventHandler OnProductHitGround;
    public event EventHandler OnProductBounced;
    public event EventHandler OnProductTaken;
    public event EventHandler OnProductPocketed;
    public event EventHandler OnInteractedWith;

    [Header("References")]
    [SerializeField] protected ProductAsset productAsset = null;
    [SerializeField] protected LayerMask playerLayerMask;

    private NetworkVariable<Vector3> networkPosition = new(Vector3.zero);

    private Interaction interaction = null;
    private BoxCollider boxCollider;
    private Vector3 velocity = Vector3.zero;
    private bool isBeingThrown = false;
    private bool isInPocket = false;

    public override void OnNetworkSpawn()
    {
        boxCollider = GetComponent<BoxCollider>();

        if (interaction == null)
        {
            Drop();
        }
    }

    protected virtual void Update()
    {
        if (interaction != null || !isBeingThrown)
        {
            return;
        }

        if (IsServer)
        {
            Vector3 origin = transform.position;
            Vector3 halfExtents = boxCollider.size * 0.5f;
            Vector3 direction = velocity.normalized;
            float distance = velocity.magnitude * Time.deltaTime;
            float minimumDistance = 0.05f;

            distance = Mathf.Clamp(distance, minimumDistance, float.MaxValue);

            bool hasCollided = Physics.BoxCast(origin, halfExtents, direction, out RaycastHit raycastHit, transform.rotation, distance, ~playerLayerMask);

            if (hasCollided)
            {
                velocity = Vector3.Reflect(velocity, raycastHit.normal) * productAsset.bounceFactor;

                OnProductBounced?.Invoke(this, EventArgs.Empty);

                bool hitWall = Vector3.Dot(Vector3.up, raycastHit.normal) <= 0.5f;

                if (velocity.magnitude <= 0.5f && !hitWall)
                {
                    isBeingThrown = false;

                    transform.forward = new Vector3(velocity.x, 0f, velocity.z).normalized;

                    velocity = Vector3.zero;

                    transform.position = raycastHit.point;
                }
            }
            else
            {
                velocity += Physics.gravity * Time.deltaTime;

                transform.position += velocity * Time.deltaTime;

                float slerpSpeed = 10f;
                transform.forward = Vector3.Slerp(transform.forward, velocity.normalized, slerpSpeed * Time.deltaTime);
            }

            networkPosition.Value = transform.position;
            //UpdateTransformClientRpc(transform.position, transform.rotation);
        }
        else
        {
            transform.position = networkPosition.Value;
        }
    }

    [ClientRpc]
    private void UpdateTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    protected virtual void LateUpdate()
    {
        if (interaction == null)
        {
            return;
        }

        if (!isInPocket)
        {
            transform.SetPositionAndRotation(interaction.GetHoldTransform().position, interaction.GetHoldTransform().rotation);
        }
        else
        {
            transform.SetPositionAndRotation(interaction.GetPocketTransform().position, interaction.GetPocketTransform().rotation);
        }
    }

    public void Drop()
    {
        DropServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropServerRpc()
    {
        Vector3 dropPosition = Vector3.zero;

        if (interaction == null)
        {
            return;
        }

        float checkForwardDistance = 0.5f;

        if (Physics.Raycast(transform.position, transform.forward, checkForwardDistance))
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit raycastHit, Mathf.Infinity, ~playerLayerMask, QueryTriggerInteraction.Ignore))
            {
                dropPosition = raycastHit.point;
            }
        }
        else
        {
            float dropForwardDistance = 0.25f;

            if (Physics.Raycast(transform.position + interaction.transform.forward * dropForwardDistance, Vector3.down, out RaycastHit raycastHit, Mathf.Infinity, ~playerLayerMask, QueryTriggerInteraction.Ignore))
            {
                dropPosition = raycastHit.point;
            }
        }

        DropClientRpc(dropPosition);
    }

    [ClientRpc]
    private void DropClientRpc(Vector3 dropPosition)
    {
        transform.position = dropPosition;
        transform.eulerAngles = interaction.transform.eulerAngles + Vector3.up * productAsset.normalYRotation;

        OnProductHitGround?.Invoke(this, EventArgs.Empty);

        ClearInteraction();
    }

    public void Throw(Vector3 direction, float force)
    {
        ThrowServerRpc(direction, force);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ThrowServerRpc(Vector3 direction, float force)
    {
        ThrowClientRpc(direction, force);
    }

    [ClientRpc]
    private void ThrowClientRpc(Vector3 direction, float force)
    {
        isBeingThrown = true;

        velocity = direction * (force / productAsset.mass);

        ClearInteraction();
    }

    public ProductAsset GetProductAsset()
    {
        return productAsset;
    }

    public bool TryGetKey(out Key key)
    {
        if (this is Key)
        {
            key = this as Key;

            return true;
        }

        key = null;

        return false;
    }

    public bool TryGetBattery(out Battery battery)
    {
        if (this is Battery)
        {
            battery = this as Battery;

            return true;
        }

        battery = null;

        return false;
    }

    public bool HasInteraction()
    {
        return interaction != null;
    }

    public void ClearInteraction()
    {
        ClearInteractionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearInteractionServerRpc()
    {
        ClearInteractionClientRpc();
    }

    [ClientRpc]
    private void ClearInteractionClientRpc()
    {
        if (interaction != null)
        {
            if (isInPocket)
            {
                interaction.ClearProductFromInventory(this);
            }
            else
            {
                interaction.ClearHeldProduct();
            }
        }

        interaction = null;
    }

    public void Delete()
    {
        ClearInteraction();

        DeleteProductServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteProductServerRpc()
    {
        NetworkObject.Despawn();
    }

    public void Interact(Interaction interaction)
    {
        if (interaction.IsHoldingProduct())
        {
            Product heldProduct = interaction.GetHeldProduct();

            if (heldProduct.TryGetComponent(out BatteryLife heldBatteryLife) && this is Battery)
            {
                Battery battery = this as Battery;

                battery.ChargeBatteryLife(heldBatteryLife);
            }

            if (heldProduct is Battery && interaction.GetSelectedInteractable().GetTransform().TryGetComponent(out BatteryLife selectedBatteryLife))
            {
                Battery battery = heldProduct as Battery;

                battery.ChargeBatteryLife(selectedBatteryLife);
            }
        }

        if (HasInteraction())
        {
            return;
        }

        InteractServerRpc(interaction.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(NetworkObjectReference interactionNetworkObjectReference)
    {
        InteractClientRpc(interactionNetworkObjectReference);
    }

    [ClientRpc]
    private void InteractClientRpc(NetworkObjectReference interactionNetworkObjectReference)
    {
        interactionNetworkObjectReference.TryGet(out NetworkObject interactionNetworkObject);
        Interaction interaction = interactionNetworkObject.GetComponent<Interaction>();

        if (interaction.TryTakeProduct(this))
        {
            this.interaction = interaction;

            OnProductTaken?.Invoke(this, EventArgs.Empty);
            OnInteractedWith?.Invoke(this, EventArgs.Empty);
        }
    }

    public string GetName()
    {
        return productAsset.displayName;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public bool IsInteractReady()
    {
        return false;
    }

    public Interaction GetInteraction()
    {
        return interaction;
    }

    public void SetIsInPocket(bool isInPocket)
    {
        this.isInPocket = isInPocket;

        boxCollider.enabled = !isInPocket;

        OnProductPocketed?.Invoke(this, EventArgs.Empty);
    }

    public bool IsInPocket()
    {
        return isInPocket;
    }
}
