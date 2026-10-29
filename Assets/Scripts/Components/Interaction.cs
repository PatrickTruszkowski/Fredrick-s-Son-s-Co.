using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HealthSystem))]
public class Interaction : NetworkBehaviour
{
    public event EventHandler OnProductDropped;
    public event EventHandler OnProductTaken;
    public event EventHandler OnProductBeganSwitching;
    public event EventHandler OnProductFinishedSwitching;
    public event EventHandler OnValidInteract;
    public event EventHandler OnCameraSystemInteract;
    public event EventHandler OnThrowReady;
    public event EventHandler OnProductThrown;

    [Header("References")]
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private Transform holdTransform = null;
    [SerializeField] private Transform pocketTransform = null;
    [SerializeField] private LayerMask ignoreInteractionLayerMask;
    [SerializeField] private ThrowTrigger throwTrigger;
    [SerializeField] private SwitchProductTrigger switchProductTrigger;

    private const float DROP_HOLD_TIMER_MAX = 0.3f;
    private const float SWITCH_TIMER_MAX = 0.1f;

    private Inventory inventory;
    private Product heldProduct = null;
    private HealthSystem playerHealthSystem;
    private IInteractable selectedInteractable = null;
    private float interactDistance = 2f;
    private float carryMass = 0f;
    private float pickupTime = 0f;
    private float pickupDuration = 0.2f;
    private float dropHoldTimer = 0f;
    private float switchTimer = 0f;
    private float throwForce = 16f;
    private int selectedProductIndex = 0;
    private bool isCloseToInteractReadyInteractable = false;
    private bool isZoomedIntoCameraSystem = false;
    private bool hasCompletedPickup = false;
    private bool isHoldingDrop = false;
    private bool isThrowReady = false;
    private bool isSwitching = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        inventory = GetComponent<Inventory>();
        playerHealthSystem = GetComponent<HealthSystem>();

        InputManager.Singleton.OnInteract += InputManager_OnInteract;
        InputManager.Singleton.OnAlternateInteract += InputManager_OnAlternateInteract;
        InputManager.Singleton.OnUse += InputManager_OnUse;
        InputManager.Singleton.OnDropPerformed += InputManager_OnDropPerformed;
        InputManager.Singleton.OnDropCanceled += InputManager_OnDropCanceled;

        playerHealthSystem.OnDeath += PlayerHealthSystem_OnDeath;

        throwTrigger.OnThrowTriggered += ThrowTrigger_OnThrowTriggered;
        switchProductTrigger.OnProductSwitchTriggered += SwitchProductTrigger_OnProductSwitchTriggered;
        switchProductTrigger.OnProductSwitchFinished += SwitchProductTrigger_OnProductSwitchFinished;
    }

    private void SwitchProductTrigger_OnProductSwitchFinished(object sender, EventArgs e)
    {
        isSwitching = false;
    }

    private void SwitchProductTrigger_OnProductSwitchTriggered(object sender, EventArgs e)
    {
        if (heldProduct != null)
        {
            heldProduct.SetIsInPocket(true);
        }

        heldProduct = inventory.GetProduct(selectedProductIndex);
        heldProduct.SetIsInPocket(false);

        OnProductFinishedSwitching?.Invoke(this, EventArgs.Empty);
    }

    private void ThrowTrigger_OnThrowTriggered(object sender, EventArgs e)
    {
        ThrowProduct();
    }

    private void PlayerHealthSystem_OnDeath(object sender, EventArgs e)
    {
        if (IsHoldingProduct())
        {
            heldProduct.Drop();

            ClearHeldProductServerRpc();
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        UpdateSelection();
        UpdateProductSwitch();
        UpdateInteractReadyCheck();
        UpdatePickupTimer();
        UpdateDropHoldTimer();
    }

    private void UpdateSelection()
    {
        Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * interactDistance, Color.yellow);

        if (!Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, interactDistance, ~ignoreInteractionLayerMask))
        {
            if (selectedInteractable != null)
            {
                selectedInteractable.OnInteractedWith -= SelectedInteractable_OnInteractedWith;
            }

            if (isZoomedIntoCameraSystem)
            {
                isZoomedIntoCameraSystem = false;
            }

            selectedInteractable = null;

            return;
        }

        if (raycastHit.transform.TryGetComponent(out IInteractable interactable))
        {
            selectedInteractable = interactable;

            if (!IsCameraSystemSelected())
            {
                isZoomedIntoCameraSystem = false;
            }

            selectedInteractable.OnInteractedWith += SelectedInteractable_OnInteractedWith;
        }
        else
        {
            if (selectedInteractable != null)
            {
                selectedInteractable.OnInteractedWith -= SelectedInteractable_OnInteractedWith;
            }

            if (isZoomedIntoCameraSystem)
            {
                isZoomedIntoCameraSystem = false;
            }

            selectedInteractable = null;
        }
    }

    private void UpdateProductSwitch()
    {
        if (isThrowReady)
        {
            return;
        }

        float scrollInput = InputManager.Singleton.GetMouseScroll();

        bool scrollUp = scrollInput > 0;
        bool scrollDown = scrollInput < 0;

        if (switchTimer > 0f)
        {
            switchTimer -= Time.deltaTime;

            return;
        }

        if (scrollDown)
        {
            if (inventory.IsEmpty() || inventory.GetProductCount() == 1)
            {
                return;
            }

            selectedProductIndex++;

            if (selectedProductIndex > inventory.GetProductCount() - 1)
            {
                selectedProductIndex = 0;
            }

            switchTimer = SWITCH_TIMER_MAX;

            SetIsSwitching();
        }
        else if (scrollUp)
        {
            if (inventory.IsEmpty() || inventory.GetProductCount() == 1)
            {
                return;
            }

            selectedProductIndex--;

            if (selectedProductIndex < 0)
            {
                selectedProductIndex = inventory.GetProductCount() - 1;
            }

            switchTimer = SWITCH_TIMER_MAX;

            SetIsSwitching();
        }

        if (inventory.IsEmpty() || inventory.GetProductCount() == 1)
        {
            return;
        }

        UpdateManualProductSelection();
    }

    private void UpdateManualProductSelection()
    {
        if (Keyboard.current[UnityEngine.InputSystem.Key.Digit1].wasPressedThisFrame)
        {
            ValidateManualProductSelection(0);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit2].wasPressedThisFrame)
        {
            ValidateManualProductSelection(1);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit3].wasPressedThisFrame)
        {
            ValidateManualProductSelection(2);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit4].wasPressedThisFrame)
        {
            ValidateManualProductSelection(3);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit5].wasPressedThisFrame)
        {
            ValidateManualProductSelection(4);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit6].wasPressedThisFrame)
        {
            ValidateManualProductSelection(5);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit7].wasPressedThisFrame)
        {
            ValidateManualProductSelection(6);
        }
        else if (Keyboard.current[UnityEngine.InputSystem.Key.Digit8].wasPressedThisFrame)
        {
            ValidateManualProductSelection(7);
        }
    }

    private void ValidateManualProductSelection(int attemptedSelectedProductIndex)
    {
        if (attemptedSelectedProductIndex <= inventory.GetProductCount() - 1 && attemptedSelectedProductIndex != selectedProductIndex)
        {
            selectedProductIndex = attemptedSelectedProductIndex;
            SetIsSwitching();
        }
    }

    private void UpdateInteractReadyCheck()
    {
        float heightOffset = 1f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.up * heightOffset, interactDistance - 0.18f, ~ignoreInteractionLayerMask);

        foreach (Collider collider in hitColliders)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.IsInteractReady())
                {
                    isCloseToInteractReadyInteractable = true;

                    return;
                }
            }
        }

        isCloseToInteractReadyInteractable = false;
    }

    private void UpdatePickupTimer()
    {
        if (!IsHoldingProduct())
        {
            return;
        }

        pickupTime += Time.deltaTime;

        if (pickupTime > pickupDuration)
        {
            hasCompletedPickup = true;
        }
    }

    private void UpdateDropHoldTimer()
    {
        if (!IsHoldingProduct() || !isHoldingDrop || isThrowReady)
        {
            return;
        }

        if (dropHoldTimer < DROP_HOLD_TIMER_MAX)
        {
            dropHoldTimer += Time.deltaTime;
        }
        else
        {
            isThrowReady = true;

            OnThrowReady?.Invoke(this, EventArgs.Empty);
        }
    }

    private void InputManager_OnDropPerformed(object sender, System.EventArgs e)
    {
        dropHoldTimer = 0f;
        isHoldingDrop = true;
    }

    private void InputManager_OnDropCanceled(object sender, System.EventArgs e)
    {
        isHoldingDrop = false;

        if (!IsHoldingProduct())
        {
            return;
        }

        if (!isThrowReady && hasCompletedPickup && !isSwitching)
        {
            heldProduct.Drop();
        }
        else
        {
            OnProductThrown?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ThrowProduct()
    {
        isThrowReady = false;

        heldProduct.Throw(playerCameraTransform.forward, !Player.LocalSingleton.IsExhausted() ? throwForce : throwForce / 2f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearHeldProductServerRpc()
    {
        ClearHeldProductClientRpc();
    }

    [ClientRpc]
    private void ClearHeldProductClientRpc()
    {
        ClearHeldProduct();
    }

    private void InputManager_OnUse(object sender, System.EventArgs e)
    {
        if (!IsOwner)
        {
            return;
        }

        if (IsHoldingProduct() && !isSwitching)
        {
            if (heldProduct.TryGetComponent(out IUseable useableProduct))
            {
                useableProduct.Use();
                OnUseServerRpc(useableProduct.GetTransform().GetComponent<IInteractable>().GetNetworkObject());
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnUseServerRpc(NetworkObjectReference useableProductNetworkObjectReference)
    {
        OnUseClientRpc(useableProductNetworkObjectReference);
    }

    [ClientRpc]
    private void OnUseClientRpc(NetworkObjectReference useableProductNetworkObjectReference)
    {
        if (IsOwner)
        {
            return;
        }

        useableProductNetworkObjectReference.TryGet(out NetworkObject useableProductNetworkObject);

        IUseable useableProduct = useableProductNetworkObject.GetComponent<IUseable>();

        useableProduct.Use();
    }

    private void InputManager_OnInteract(object sender, System.EventArgs e)
    {
        if (selectedInteractable == null)
        {
            return;
        }

        // Change this probably... Thinking about making the buttons on the monitor the switch.
        if (IsCameraSystemSelected())
        {
            if (!isZoomedIntoCameraSystem)
            {
                isZoomedIntoCameraSystem = true;
            }
            else
            {
                selectedInteractable.Interact(this);
            }
        }
        else
        {
            selectedInteractable.Interact(this);
        }
    }

    private void InputManager_OnAlternateInteract(object sender, EventArgs e)
    {
        if (IsHoldingProduct())
        {
            if (heldProduct.TryGetComponent(out IAlternateInteractable alternateInteractable))
            {
                alternateInteractable.AlternateInteract(this);
                OnAlternateInteractServerRpc(alternateInteractable.GetTransform().GetComponent<IInteractable>().GetNetworkObject());
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnAlternateInteractServerRpc(NetworkObjectReference alternateInteractableNetworkObjectReference)
    {
        OnAlternateInteractClientRpc(alternateInteractableNetworkObjectReference);
    }

    [ClientRpc]
    private void OnAlternateInteractClientRpc(NetworkObjectReference alternateInteractableNetworkObjectReference)
    {
        if (IsOwner)
        {
            return;
        }

        alternateInteractableNetworkObjectReference.TryGet(out NetworkObject alternateInteractableNetworkObject);

        IAlternateInteractable alternateInteractable = alternateInteractableNetworkObject.GetComponent<IAlternateInteractable>();

        alternateInteractable.AlternateInteract(this);
    }

    private void SelectedInteractable_OnInteractedWith(object sender, EventArgs e)
    {
        if (sender is Product)
        {
            return;
        }

        if (sender is Trash)
        {
            return;
        }

        if (sender is CameraSystem)
        {
            return;
        }

        OnValidInteract?.Invoke(this, EventArgs.Empty);
    }

    private bool IsCameraSystemSelected()
    {
        return selectedInteractable != null && selectedInteractable is CameraSystem;
    }

    private void SetIsSwitching()
    {
        isSwitching = true;
        OnProductBeganSwitching?.Invoke(this, EventArgs.Empty);
    }

    public bool IsHoldingProduct()
    {
        return heldProduct != null;
    }

    public bool TryTakeProduct(Product product)
    {
        if (inventory.TryAddProduct(product))
        {
            if (heldProduct == null)
            {
                heldProduct = product;
            }
            else
            {
                product.SetIsInPocket(true);
            }

            carryMass += product.GetProductAsset().mass;

            OnProductTaken?.Invoke(this, EventArgs.Empty);

            return true;
        }

        return false;
    }

    public Product GetHeldProduct()
    {
        return heldProduct;
    }

    public void ClearHeldProduct()
    {
        inventory.RemoveProduct(selectedProductIndex);

        hasCompletedPickup = false;
        isHoldingDrop = false;
        isThrowReady = false;
        pickupTime = 0;

        carryMass = Mathf.Clamp(carryMass - heldProduct.GetProductAsset().mass, 0f, float.MaxValue);

        if (!inventory.IsEmpty())
        {
            selectedProductIndex--;

            if (selectedProductIndex < 0)
            {
                selectedProductIndex = 0;
            }

            heldProduct = null;

            SetIsSwitching();
        }
        else
        {
            heldProduct = null;
        }

        OnProductDropped?.Invoke(this, EventArgs.Empty);
    }

    public void ClearProductFromInventory(Product product)
    {
        inventory.RemoveProduct(product);

        carryMass = Mathf.Clamp(carryMass - product.GetProductAsset().mass, 0f, float.MaxValue);
    }

    public float GetInteractDistance()
    {
        return interactDistance;
    }

    public float GetCarryMass()
    {
        return carryMass;
    }

    public bool IsOverAnyInteractable(bool includeProducts = true)
    {
        if (includeProducts)
        {
            return selectedInteractable != null;
        }

        return selectedInteractable != null && selectedInteractable is not Product;
    }

    public IInteractable GetSelectedInteractable()
    {
        return selectedInteractable;
    }

    public Transform GetHoldTransform()
    {
        return holdTransform;
    }

    public Transform GetPocketTransform()
    {
        return pocketTransform;
    }

    public bool IsZoomedIntoCameraSystem()
    {
        return isZoomedIntoCameraSystem;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public bool IsHeldProductUseable()
    {
        return heldProduct != null && heldProduct is IUseable;
    }

    public bool IsCloseToInteractReadyInteractable()
    {
        return isCloseToInteractReadyInteractable;
    }

    public bool IsSwitching()
    {
        return isSwitching;
    }

    public Transform GetPlayerCameraTransform()
    {
        return playerCameraTransform;
    }

    public int GetSelectedProductIndex()
    {
        return selectedProductIndex;
    }
}
