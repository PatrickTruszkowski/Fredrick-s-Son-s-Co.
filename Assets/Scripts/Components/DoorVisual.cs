using UnityEngine;

public class DoorVisual : MonoBehaviour
{
    [SerializeField] private ButtonSwitch buttonSwitch;

    private const string CLOSE_DOOR = "CloseDoor";
    private const string OPEN_DOOR = "OpenDoor";

    private Animator animator;
    private AudioSource audioSource;

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

    private void ButtonSwitch_OnButtonPressed(object sender, System.EventArgs e)
    {
        if (!Building.Singleton.HasPower())
        {
            return;
        }

        if (buttonSwitch.IsOpen())
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void Building_OnBuildingPowerLost(object sender, System.EventArgs e)
    {
        OpenDoor();
    }

    private void OpenDoor()
    {
        animator.SetTrigger(OPEN_DOOR);
        audioSource.PlayOneShot(audioSource.clip);

        PlayerCameraShaker.LocalSingleton.Shake(transform.position, ShakeIntensity.Soft, 0.2f);
    }

    private void CloseDoor()
    {
        animator.SetTrigger(CLOSE_DOOR);
        audioSource.PlayOneShot(audioSource.clip);

        PlayerCameraShaker.LocalSingleton.Shake(transform.position, ShakeIntensity.Soft, 0.2f);
    }
}
