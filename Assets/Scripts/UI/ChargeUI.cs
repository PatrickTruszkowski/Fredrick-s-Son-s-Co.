using UnityEngine;
using UnityEngine.UI;

public class ChargeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Interaction interaction = null;
    [SerializeField] private GameObject uiItems = null;
    [SerializeField] private Image chargeBar = null;

    private BatteryLife selectedBatteryLife = null;

    private void Awake()
    {
        uiItems.SetActive(false);
    }

    private void Update()
    {
        if (interaction.IsOverAnyInteractable())
        {
            IInteractable selectedInteractable = interaction.GetSelectedInteractable();

            if (interaction.IsHoldingProduct() && interaction.GetHeldProduct().TryGetBattery(out _) && selectedInteractable.GetTransform().TryGetComponent(out BatteryLife batteryLife))
            {
                selectedBatteryLife = batteryLife;
            }
            else
            {
                selectedBatteryLife = null;
            }
        }
        else
        {
            selectedBatteryLife = null;
        }

        if (selectedBatteryLife != null)
        {
            chargeBar.fillAmount = selectedBatteryLife.GetCharge();

            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        uiItems.SetActive(true);
    }

    private void Hide()
    {
        uiItems.SetActive(false);
    }
}
