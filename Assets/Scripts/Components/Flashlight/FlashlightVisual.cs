using UnityEngine;

public class FlashlightVisual : MonoBehaviour
{
    [SerializeField] private Flashlight flashlight = null;
    [SerializeField] private GameObject[] lightVisualGameObjects = null;

    private void Start()
    {
        flashlight.OnUsed += Flashlight_OnFlashlightToggled;

        flashlight.GetBatteryLife().OnBatteryLifeDied += FlashlightVisual_OnBatteryLifeDead;
        flashlight.GetBatteryLife().OnBatteryLifeCharged += FlashlightVisual_OnBatteryLifeCharged;
    }

    private void Flashlight_OnFlashlightToggled(object sender, System.EventArgs e)
    {
        if (flashlight.IsProductOn())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void FlashlightVisual_OnBatteryLifeDead(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void FlashlightVisual_OnBatteryLifeCharged(object sender, System.EventArgs e)
    {
        if (flashlight.IsProductOn())
        {
            Show();
        }
    }

    private void Show()
    {
        foreach (GameObject lightVisualGameObject in lightVisualGameObjects)
        {
            lightVisualGameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (GameObject lightVisualGameObject in lightVisualGameObjects)
        {
            lightVisualGameObject.SetActive(false);
        }
    }
}
