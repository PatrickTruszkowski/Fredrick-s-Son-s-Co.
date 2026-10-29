using UnityEngine;

public class RadioAudioSource : MonoBehaviour
{
    [SerializeField] private Radio radio = null;

    private AudioSource audioSource = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        radio.OnUsed += Radio_OnUsed;
        radio.OnAlternateInteract += Radio_OnAlternateInteract;

        radio.GetBatteryLife().OnBatteryLifeDied += RadioAudioSource_OnBatteryLifeDied;
        radio.GetBatteryLife().OnBatteryLifeCharged += RadioAudioSource_OnBatteryLifeCharged;
    }

    private void Radio_OnUsed(object sender, System.EventArgs e)
    {
        audioSource.clip = radio.GetCurrentSongAudioClip();

        if (radio.IsProductOn())
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Radio_OnAlternateInteract(object sender, System.EventArgs e)
    {
        bool isProductOn = radio.IsProductOn();

        if (isProductOn)
        {
            audioSource.Pause();
        }

        audioSource.clip = radio.GetCurrentSongAudioClip();

        if (isProductOn)
        {
            audioSource.Play();
        }
    }

    private void RadioAudioSource_OnBatteryLifeDied(object sender, System.EventArgs e)
    {
        audioSource.Pause();
    }

    private void RadioAudioSource_OnBatteryLifeCharged(object sender, System.EventArgs e)
    {
        if (radio.IsProductOn() && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
