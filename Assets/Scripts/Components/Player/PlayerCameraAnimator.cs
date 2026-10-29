using SpaceDoktor.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCameraAnimator : NetworkBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Interaction interaction;
    [SerializeField] private HealthSystem playerHealthSystem;

    private const string IS_MOVING = "IsMoving";
    private const string HAS_MOVE_INPUT = "HasMoveInput";
    private const string IS_RUNNING = "IsRunning";
    private const string IS_CROUCHING = "IsCrouching";
    private const string IS_SLIDING = "IsSliding";
    private const string IS_AIRBORN = "IsAirborn";
    private const string DIE = "Die";

    private Animator playerCameraAnimator;
    private float lerpedCrossProduct;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private ChannelMixer channelMixer;
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;
    private float regularFieldOfView = 0f;
    private float runningFieldOfView = 0f;
    private float normalSaturationValue = 0f;
    private Volume postProccessVolume;
    private bool hasBloodyScreen = false;
    private bool hasDamagedScreen = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        playerCameraAnimator = GetComponent<Animator>();

        regularFieldOfView = playerCamera.fieldOfView;

        float fieldOfViewDifference = 4f;

        runningFieldOfView = regularFieldOfView + fieldOfViewDifference;

        normalSaturationValue = 5f;

        postProccessVolume = LocalPostProccessVolume.LocalSingleton.volume;

        if (postProccessVolume.profile.TryGet(out Vignette vignette))
        {
            this.vignette = vignette;
        }

        if (postProccessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            this.colorAdjustments = colorAdjustments;
        }

        if (postProccessVolume.profile.TryGet(out ChannelMixer channelMixer))
        {
            this.channelMixer = channelMixer;
        }

        if (postProccessVolume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            this.chromaticAberration = chromaticAberration;
        }

        if (postProccessVolume.profile.TryGet(out DepthOfField depthOfField))
        {
            this.depthOfField = depthOfField;
        }

        playerHealthSystem.OnHitCountChanged += PlayerHealthSystem_OnHitCountChanged;
        playerHealthSystem.OnDamaged += PlayerHealthSystem_OnDamaged;
        playerHealthSystem.OnDeath += PlayerHealthSystem_OnDeath;
    }

    private void PlayerHealthSystem_OnDeath(object sender, System.EventArgs e)
    {
        playerCameraAnimator.SetTrigger(DIE);
    }

    private void PlayerHealthSystem_OnDamaged(object sender, System.EventArgs e)
    {
        // Also include the trigger to player the hit animation.

        PlayerCameraShaker.LocalSingleton.Shake(transform.position, ShakeIntensity.Moderate, 0.5f);
    }

    private void PlayerHealthSystem_OnHitCountChanged(object sender, System.EventArgs e)
    {
        DamageStrength currentDamageStrength = playerHealthSystem.GetCurrentDamageStrength();

        if (currentDamageStrength > DamageStrength.Normal)
        {
            hasBloodyScreen = true;
            hasDamagedScreen = true;
        }
        else
        {
            if (currentDamageStrength < DamageStrength.Normal)
            {
                hasDamagedScreen = false;
            }
            else
            {
                hasDamagedScreen = true;
            }

            hasBloodyScreen = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        UpdateCameraTilt();
        UpdateCameraAnimations();
        UpdateCameraFieldOfView();
        UpdatePostProccessingEffects();
    }

    private void UpdateCameraTilt()
    {
        float cameraAngleLerpMovingMultiplier = 3f;
        float cameraAngleLerpStoppingMultiplier = 10f;

        if (player.HasMoveInput() && player.IsPressingRunKey() && player.CanRun() && !player.IsCrouching())
        {
            float crossProduct;

            if (player.GetInputVector().x == 0f)
            {
                crossProduct = player.GetVelocity().normalized.x * transform.forward.z - player.GetVelocity().normalized.z * transform.forward.x;
            }
            else
            {
                Vector3 inputDirection = transform.right * -player.GetInputVector().x + transform.forward * player.GetInputVector().y;

                crossProduct = inputDirection.x * transform.forward.z - inputDirection.z * transform.forward.x;
            }

            lerpedCrossProduct = Smoothing.Exponential(lerpedCrossProduct, crossProduct, cameraAngleLerpMovingMultiplier, Time.deltaTime);
        }
        else
        {
            lerpedCrossProduct = Smoothing.Exponential(lerpedCrossProduct, 0f, cameraAngleLerpStoppingMultiplier, Time.deltaTime);
        }

        float maximumTiltAngle = 5f;

        playerCamera.transform.localEulerAngles = playerCamera.transform.localEulerAngles + lerpedCrossProduct * maximumTiltAngle * Vector3.forward;
    }

    private void UpdateCameraAnimations()
    {
        if (!player.IsCloseToGround())
        {
            playerCameraAnimator.SetBool(IS_AIRBORN, true);

            return;
        }

        playerCameraAnimator.SetBool(IS_AIRBORN, false);

        float minimumAnimationSpeed = 0.6f;
        float maximumAnimationSpeed = 1.3f;

        if (player.IsCloseToGround() && !player.IsCrouching() && player.IsMoving() && player.IsPressingRunKey() && player.CanRun())
        {
            float maximumVelocity = 5.8f;

            playerCameraAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / maximumVelocity, minimumAnimationSpeed, maximumAnimationSpeed);
        }
        else if (player.IsCloseToGround() && !player.IsCrouching() && player.IsMoving())
        {
            playerCameraAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / 2.1f, minimumAnimationSpeed, maximumAnimationSpeed);
        }
        else
        {
            playerCameraAnimator.speed = 1f;
        }

        playerCameraAnimator.SetBool(IS_MOVING, player.IsMoving());
        playerCameraAnimator.SetBool(HAS_MOVE_INPUT, player.HasMoveInput());
        playerCameraAnimator.SetBool(IS_RUNNING, player.IsPressingRunKey() && player.CanRun());
        playerCameraAnimator.SetBool(IS_CROUCHING, player.IsCrouching() || player.IsSliding());
        playerCameraAnimator.SetBool(IS_SLIDING, player.IsSliding());
    }

    private void UpdateCameraFieldOfView()
    {
        if (interaction.IsZoomedIntoCameraSystem())
        {
            float newFieldOfView = 20f;
            float zoomSpeed = 16f;

            playerCamera.fieldOfView = Smoothing.Exponential(playerCamera.fieldOfView, newFieldOfView, zoomSpeed, Time.deltaTime);
        }
        else
        {
            float fieldOfViewChange = 6f;

            if ((player.IsMoving() && !interaction.IsZoomedIntoCameraSystem() && player.IsPressingRunKey() && player.CanRun()) || player.IsSliding())
            {
                playerCamera.fieldOfView = Smoothing.Exponential(playerCamera.fieldOfView, runningFieldOfView, fieldOfViewChange, Time.deltaTime);
            }
            else if (!player.IsPressingRunKey() || !interaction.IsZoomedIntoCameraSystem())
            {
                playerCamera.fieldOfView = Smoothing.Exponential(playerCamera.fieldOfView, regularFieldOfView, fieldOfViewChange, Time.deltaTime);
            }
        }
    }

    private void UpdatePostProccessingEffects()
    {
        float effectLerpMultiplier = 4f;
        float normalVignetteIntensity = 0.3f;
        float tiredSaturationValue = -50f;
        float blurLerpSpeed = 32f;

        if (player.IsExhausted())
        {
            playerCameraAnimator.speed = 0.75f;

            vignette.intensity.value = Smoothing.Exponential(vignette.intensity.value, Random.Range(0.5f, 0.6f), effectLerpMultiplier, Time.deltaTime);
            colorAdjustments.saturation.value = Smoothing.Exponential(colorAdjustments.saturation.value, tiredSaturationValue, effectLerpMultiplier, Time.deltaTime);
        }
        else
        {
            vignette.intensity.value = Smoothing.Exponential(vignette.intensity.value, normalVignetteIntensity, effectLerpMultiplier, Time.deltaTime);
            colorAdjustments.saturation.value = Smoothing.Exponential(colorAdjustments.saturation.value, normalSaturationValue, effectLerpMultiplier, Time.deltaTime);
        }

        if (hasDamagedScreen)
        {
            vignette.color.value = Color.Lerp(vignette.color.value, Color.red, effectLerpMultiplier * Time.deltaTime);
            chromaticAberration.intensity.value = Smoothing.Exponential(chromaticAberration.intensity.value, 1f, effectLerpMultiplier, Time.deltaTime);
        }
        else
        {
            vignette.color.value = Color.Lerp(vignette.color.value, Color.black, effectLerpMultiplier * Time.deltaTime);
            chromaticAberration.intensity.value = Smoothing.Exponential(chromaticAberration.intensity.value, 0f, effectLerpMultiplier, Time.deltaTime);
        }

        if (hasBloodyScreen)
        {
            vignette.intensity.value = Smoothing.Exponential(vignette.intensity.value, 0.5f, effectLerpMultiplier, Time.deltaTime);
            channelMixer.redOutRedIn.value = Smoothing.Exponential(channelMixer.redOutRedIn.value, 200f, effectLerpMultiplier, Time.deltaTime);
            depthOfField.gaussianStart.value = Smoothing.Exponential(depthOfField.gaussianStart.value, 0f, blurLerpSpeed, Time.deltaTime);
        }
        else
        {
            vignette.intensity.value = Smoothing.Exponential(vignette.intensity.value, normalVignetteIntensity, effectLerpMultiplier, Time.deltaTime);
            channelMixer.redOutRedIn.value = Smoothing.Exponential(channelMixer.redOutRedIn.value, 100f, effectLerpMultiplier, Time.deltaTime);
            depthOfField.gaussianStart.value = Smoothing.Exponential(depthOfField.gaussianStart.value, 100000f, 1f, Time.deltaTime);
        }
    }
}
