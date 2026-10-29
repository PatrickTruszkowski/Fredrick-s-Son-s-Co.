using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Interaction interaction = null;
    [SerializeField] private Inventory inventory = null;
    [SerializeField] private TextMeshProUGUI interactionText = null;
    [SerializeField] private TextMeshProUGUI seedText = null;
    [SerializeField] private GameObject interactionButtonTextGameObject = null;
    [SerializeField] private GameObject dropProductText = null;
    [SerializeField] private GameObject throwProductText = null;
    [SerializeField] private GameObject useProductText = null;
    [SerializeField] private Image crosshair = null;

    [Header("Inventory")]
    [SerializeField] private GameObject[] gridSlotArray = new GameObject[MAX_SLOT_COUNT];
    [SerializeField] private Transform inventoryParentTransform;

    private const int MAX_SLOT_COUNT = 8;
    private const int SLOT_SIZE_X = 100;
    private const int SLOT_HALF_SIZE_X = 50;
    private const float SLOT_HIDDEN_ALPHA = 32f;

    private List<Transform> hotBarImageTransformList = new(MAX_SLOT_COUNT);
    private Color showColor;
    private Color hideColor;
    private Vector3 baseSlotPosition;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);

            return;
        }

        showColor = new Color(1, 1, 1, 1);
        hideColor = new Color(1, 1, 1, 0.1f);

        baseSlotPosition = gridSlotArray[0].transform.localPosition;

        inventory.OnProductAdded += Interaction_OnProductTaken;
        interaction.OnProductDropped += Interaction_OnProductDropped;

        inventory.OnProductAdded += Inventory_OnProductAdded;
        inventory.OnProductRemoved += Inventory_OnProductRemoved;

        if (IsServer)
        {
            BasementGenerator.Singleton.networkSeed.OnValueChanged += (previousValue, newValue) =>
           {
               seedText.text = newValue.ToString();
           };
        }
        else
        {
            seedText.text = BasementGenerator.Singleton.networkSeed.Value.ToString();
        }

        Hide();
        HidePlayerGuideKeys();
    }

    private void Inventory_OnProductRemoved(object sender, Inventory.OnProductAddRemoveEventArgs e)
    {
        UpdateGridSlots(false, e.product, e.productIndex);
    }

    private void Inventory_OnProductAdded(object sender, Inventory.OnProductAddRemoveEventArgs e)
    {
        UpdateGridSlots(true, e.product, e.productIndex);
    }

    private void Interaction_OnProductDropped(object sender, System.EventArgs e)
    {
        HidePlayerGuideKeys();
    }

    private void Interaction_OnProductTaken(object sender, System.EventArgs e)
    {
        ShowPlayerGuideKeys();
    }

    private void Update()
    {
        UpdateCrosshair();
        UpdateSelectedProductHotbar();
    }

    private void UpdateCrosshair()
    {
        if (interaction.IsOverAnyInteractable())
        {
            if (interaction.GetSelectedInteractable() is not Product)
            {
                Show();

                return;
            }

            Product selectedProduct = interaction.GetSelectedInteractable() as Product;

            if (!selectedProduct.HasInteraction())
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
        else
        {
            Hide();
        }
    }

    private void UpdateSelectedProductHotbar()
    {
        int selectedProductIndex = interaction.GetSelectedProductIndex();
        float lerpSpeed = 12f;

        for (int i = 0; i < hotBarImageTransformList.Count; i++)
        {
            HotbarImage currentHotbarImage = hotBarImageTransformList[i].GetComponent<HotbarImage>();
            List<Image> hotbarImageList = currentHotbarImage.imageList;

            Product currentProduct = inventory.GetProduct(i);

            if (currentProduct.TryGetComponent(out BatteryLife batteryLife))
            {
                currentHotbarImage.chargeFillImage.fillAmount = batteryLife.GetCharge();
            }

            if (selectedProductIndex != i) // Not the currently selected product.
            {
                foreach (Image hotbarImage in hotbarImageList)
                {
                    // If the current image is a charge element.
                    if (!((hotbarImage == currentHotbarImage.chargeFillImage || hotbarImage == currentHotbarImage.chargeFillImageBackground) && batteryLife && batteryLife.GetUseableProduct().IsProductOn()))
                    {
                        hotbarImage.color = Color.Lerp(hotbarImage.color, new Color(hotbarImage.color.r, hotbarImage.color.g, hotbarImage.color.b, SLOT_HIDDEN_ALPHA / 255f), lerpSpeed * Time.deltaTime);
                    }
                }
            }
            else
            {
                foreach (Image hotbarImage in hotbarImageList)
                {
                    hotbarImage.color = Color.Lerp(hotbarImage.color, new Color(hotbarImage.color.r, hotbarImage.color.g, hotbarImage.color.b, 1f), lerpSpeed * Time.deltaTime);
                }
            }
        }
    }

    private void Show()
    {
        interactionText.text = interaction.GetSelectedInteractable().GetName();

        crosshair.color = showColor;

        interactionButtonTextGameObject.SetActive(true);
    }

    private void Hide()
    {
        interactionText.text = "";

        crosshair.color = hideColor;

        interactionButtonTextGameObject.SetActive(false);
    }

    private void ShowPlayerGuideKeys()
    {
        dropProductText.SetActive(true);
        throwProductText.SetActive(true);

        if (interaction.IsHeldProductUseable())
        {
            useProductText.SetActive(true);
        }
    }

    private void HidePlayerGuideKeys()
    {
        dropProductText.SetActive(false);
        throwProductText.SetActive(false);
        useProductText.SetActive(false);
    }

    private void UpdateGridSlots(bool isAdding, Product product, int productIndex)
    {
        ProductAsset productAsset = product.GetProductAsset();

        if (isAdding)
        {
            Transform hotbarImageTransform = Instantiate(productAsset.hotbarImage, inventoryParentTransform);
            hotBarImageTransformList.Add(hotbarImageTransform);

            float offsetX = ((inventory.GetSlotsFilledCount() - productAsset.size) * SLOT_SIZE_X) + ((productAsset.size - 1) * SLOT_HALF_SIZE_X);
            hotbarImageTransform.transform.localPosition = baseSlotPosition + Vector3.right * offsetX;
        }
        else
        {
            Destroy(hotBarImageTransformList[productIndex].gameObject);
            hotBarImageTransformList.RemoveAt(productIndex);

            for (int i = productIndex; i < hotBarImageTransformList.Count; i++)
            {
                hotBarImageTransformList[i].transform.localPosition -= Vector3.right  * (productAsset.size * SLOT_SIZE_X);
            }
        }

        for (int i = 0; i < MAX_SLOT_COUNT; i++)
        {
            if (i < inventory.GetSlotsFilledCount())
            {
                gridSlotArray[i].SetActive(false);
            }
            else
            {
                gridSlotArray[i].SetActive(true);
            }
        }
    }
}
