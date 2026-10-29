using UnityEngine;

public class PlayerLocalScreenAudio : MonoBehaviour
{
    [SerializeField] private HealthSystem playerHealthSystem;
    [SerializeField] private float hearingLossFrequency = 100f;

    private const float NORMAL_HEARING_FREQUENCY = 5000f;

    private AudioLowPassFilter playerLowPassFilter;
    private bool hasHearingLoss = false;
    private float frequencyVelocity = 0f;
    private AudioSource screenAudioSource;
    private float earRingVolumeVelocity = 0f;

    private void Start()
    {
        screenAudioSource = GetComponent<AudioSource>();

        playerHealthSystem.OnHitCountChanged += PlayerHealthSystem_OnHitCountChanged;
    }

    private void PlayerHealthSystem_OnHitCountChanged(object sender, System.EventArgs e)
    {
        if (playerHealthSystem.GetCurrentDamageStrength() == DamageStrength.Heavy || playerHealthSystem.GetCurrentDamageStrength() == DamageStrength.Deadly)
        {
            hasHearingLoss = true;
        }
        else
        {
            hasHearingLoss = false;
        }
    }

    private void Awake()
    {
        playerLowPassFilter = GetComponent<AudioLowPassFilter>();
    }

    private void Update()
    {
        if (hasHearingLoss)
        {
            float speed = 0.05f;

            playerLowPassFilter.cutoffFrequency = Mathf.SmoothDamp(playerLowPassFilter.cutoffFrequency, hearingLossFrequency, ref frequencyVelocity, speed);

            screenAudioSource.volume = 0.04f;
        }
        else
        {
            float cutoffFrequencySpeed = 3f;
            float smoothingSpeed = 1.1f;

            playerLowPassFilter.cutoffFrequency = Mathf.SmoothDamp(playerLowPassFilter.cutoffFrequency, NORMAL_HEARING_FREQUENCY, ref frequencyVelocity, cutoffFrequencySpeed);

            screenAudioSource.volume = Mathf.SmoothDamp(screenAudioSource.volume, 0f, ref earRingVolumeVelocity, smoothingSpeed);
        }
    }
}
