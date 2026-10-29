using UnityEngine;

public class ButtonSwitchVisual : MonoBehaviour
{
    [SerializeField] private ButtonSwitch buttonSwitch = null;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonNoPower;

    private const string PRESSED = "Pressed";
    private const string UNPRESSED = "Unpressed";

    private Animator animator = null;
    private AudioSource audioSource = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        buttonSwitch.OnButtonPressed += ButtonSwitch_OnButtonPressed;

        Building.Singleton.OnBuildingPowerLost += Building_OnBuildingPowerLost;
    }

    private void Building_OnBuildingPowerLost(object sender, System.EventArgs e)
    {
        ButtonPress(false, false);
    }

    private void ButtonSwitch_OnButtonPressed(object sender, System.EventArgs e)
    {
        if (!buttonSwitch.IsOpen())
        {
            ButtonPress(true, Building.Singleton.HasPower());
        }
        else
        {
            ButtonPress(false, Building.Singleton.HasPower());
        }        
    }

    private void ButtonPress(bool isPressed, bool buildingHasPower)
    {
        if (isPressed && buildingHasPower)
        {
            animator.SetTrigger(PRESSED);
        }
        else
        {
            animator.SetTrigger(UNPRESSED);
        }

        if (buildingHasPower)
        {
            audioSource.PlayOneShot(buttonClick);
        }
        else
        {
            audioSource.PlayOneShot(buttonNoPower);
        }
    }
}
