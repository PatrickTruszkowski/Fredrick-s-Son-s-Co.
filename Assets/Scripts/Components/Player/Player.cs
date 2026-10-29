using System;
using SpaceDoktor.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class Player : NetworkBehaviour
{
    public static event EventHandler OnAnyPlayerSpawned;

    public static Player LocalSingleton { get; private set; }

    public event EventHandler OnPlayerExhausted;
    public event EventHandler OnPlayerStoppedSliding;

    [Header("References")]
    [SerializeField] private Camera playerCamera = null;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private GameObject localPlayerLight;

    [Header("Movement Parameters")]
    [SerializeField] private float baseMoveSpeed = 2.25f;
    [SerializeField] private float runSpeedMultiplier = 3f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float mass = 80f;

    [Header("Camera Parameters")]
    [SerializeField][Range(0.01f, 2f)] private float cameraSensitivity = 0.08f;
    [SerializeField][Range(16f, 64f)] private float gamepadCameraSensitivity = 24f;
    [SerializeField] private RawImage pixelatedRenderTextureImage = null;

    [Header("Debug")]
    [SerializeField] private bool isTesting = false;

    private const float CROUCH_SPEED_MULTIPLIER = 0.5f;
    private const float EXHAUSTED_SPEED_MULTIPLIER = 0.75f;
    private const float MIN_RUN_LAG = 0.05f;
    private const float MAX_RUN_LAG = 3f;
    private const float MIN_RUN_SHARPNESS = 0.5f;
    private const float MAX_RUN_SHARPNESS = 3f;
    private const float MAX_NORMAL_MOVE_DELTA = 20f;
    private const float MIN_NORMAL_MOVE_DELTA = 5f;

    private CharacterController characterController = null;
    private Interaction interaction = null;
    private HealthSystem healthSystem;

    private RaycastHit groundInformation;

    private Vector2 inputVector = Vector2.zero;
    private Vector2 mouseDelta = Vector2.zero;

    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothDampRunVelocity = Vector3.zero;
    private Vector3 previousPosition;

    private float currentMoveSpeed = 0f;
    private float currentRunSharpness = MAX_RUN_SHARPNESS;
    private float currentRunLag = MIN_RUN_LAG;
    private float currentNormalMoveDelta = MAX_NORMAL_MOVE_DELTA;
    private float currentGravity = 0f;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private float stamina = 100f;
    private float exhaustedCoolDown = 4f;
    private float exhaustedTimer = 0f;

    private bool isPressingRunKey = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool wantsToSlide = false;
    private bool wantsToStopSliding = false;
    private bool shouldSlideBoost = true;
    private bool isHeadBlocked = false;
    private bool hasMoveInput = false;
    private bool isMoving = false;
    private bool canRun = true;
    private bool isCloseToGround = true;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerCamera.transform.gameObject.SetActive(false);
            localPlayerLight.SetActive(false);

            return;
        }

        LocalSingleton = this;

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (OwnerClientId == 0)
        {
            transform.position = BasementGenerator.Singleton.transform.position + new Vector3(4f, 2f, -4f); // Spawn player in the middle of the basement where the elevator would be.
            //transform.position = new Vector3(0f, 0f, 0f);

            cameraYaw = 135f;
        }
        else
        {
            transform.position = BasementGenerator.Singleton.transform.position + new Vector3(6f, 2f, -4f);

            cameraYaw = 225f;
        }

        playerCamera.targetTexture = MultiplayerManager.Singleton.GetPlayerRenderTextureArray()[OwnerClientId];
        pixelatedRenderTextureImage.texture = playerCamera.targetTexture;

        characterController = GetComponent<CharacterController>();
        interaction = GetComponent<Interaction>();
        healthSystem = GetComponent<HealthSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputManager.Singleton.OnCrouch += InputManager_OnCrouch;

        interaction.OnProductTaken += Interaction_OnProductTaken;
        interaction.OnProductDropped += Interaction_OnProductDropped;
    }

    private void Interaction_OnProductDropped(object sender, EventArgs e)
    {
        UpdateMovementProperties();
    }

    private void Interaction_OnProductTaken(object sender, EventArgs e)
    {
        UpdateMovementProperties();
    }

    private void InputManager_OnCrouch(object sender, EventArgs e)
    {
        if (healthSystem.IsDead())
        {
            return;
        }

        float minimumStamina = 1f;
        if (isPressingRunKey && stamina > minimumStamina)
        {
            wantsToSlide = true;
        }

        if (isSliding)
        {
            wantsToStopSliding = true;
        }

        isCrouching = !isCrouching;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // This is test code.
        if (healthSystem.IsDead())
        {
            float slerpSpeed = 4f;

            playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, Quaternion.identity, slerpSpeed * Time.deltaTime);

            return;
        }

        UpdateInput();
        UpdateStamina();
        UpdateMovement();
        UpdateLook();
    }


    private void UpdateInput()
    {
        inputVector = InputManager.Singleton.NormalizedInputVector();

        if (!interaction.IsZoomedIntoCameraSystem())
        {
            if (!InputManager.Singleton.IsActiveDeviceController())
            {
                mouseDelta = InputManager.Singleton.InvertedMouseDeltaVector() * cameraSensitivity;
            }
            else
            {
                mouseDelta = cameraSensitivity * gamepadCameraSensitivity * InputManager.Singleton.InvertedMouseDeltaVector();
            }
        }
        else
        {
            if (!InputManager.Singleton.IsActiveDeviceController())
            {
                mouseDelta = InputManager.Singleton.InvertedMouseDeltaVector() * (cameraSensitivity * 0.25f);
            }
            else
            {
                mouseDelta = InputManager.Singleton.InvertedMouseDeltaVector() * (cameraSensitivity * gamepadCameraSensitivity * 0.25f);
            }
        }

        isPressingRunKey = InputManager.Singleton.IsPressingRun();

        hasMoveInput = inputVector != Vector2.zero;
    }

    private void UpdateStamina()
    {
        if (isTesting) return;

        if (stamina == 0f)
        {
            canRun = false;
            exhaustedTimer = exhaustedCoolDown;

            OnPlayerExhausted?.Invoke(this, EventArgs.Empty);
        }

        if (exhaustedTimer > 0f)
        {
            exhaustedTimer -= Time.deltaTime;
        }

        float rayLength = 0.25f;
        isCloseToGround = Physics.SphereCast(transform.position + Vector3.up * characterController.radius, characterController.radius, -transform.up, out groundInformation, rayLength, ~playerLayerMask);

        if (isPressingRunKey && !isHeadBlocked && !isSliding && isCloseToGround && isMoving && exhaustedTimer <= 0f)
        {
            float staminaDepletionAmount = 8.25f;

            canRun = true;
            stamina -= staminaDepletionAmount * Time.deltaTime;
        }
        else
        {
            float staminaRegenerationAmount = 7.5f;

            stamina += staminaRegenerationAmount * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, 100f);
    }

    private void UpdateMovement()
    {
        if (isPressingRunKey && hasMoveInput)
        {
            isCrouching = false;
        }

        if (isPressingRunKey && canRun && !isSliding)
        {
            currentMoveSpeed = Smoothing.Exponential(currentMoveSpeed, baseMoveSpeed * runSpeedMultiplier, currentRunSharpness, Time.deltaTime);
        }
        else if (isCrouching)
        {
            float sharpness = 16f;

            currentMoveSpeed = Smoothing.Exponential(currentMoveSpeed, baseMoveSpeed * CROUCH_SPEED_MULTIPLIER, sharpness, Time.deltaTime);
        }
        else if (IsExhausted())
        {
            float sharpness = 16f;

            currentMoveSpeed = Smoothing.Exponential(currentMoveSpeed, baseMoveSpeed * EXHAUSTED_SPEED_MULTIPLIER, sharpness, Time.deltaTime);
        }
        else
        {
            float sharpness = 16f;

            currentMoveSpeed = Smoothing.Exponential(currentMoveSpeed, baseMoveSpeed, sharpness, Time.deltaTime);
        }

        bool isOnSlope = false;

        if (isCloseToGround)
        {
            isOnSlope = Vector3.Dot(Vector3.up, groundInformation.normal) < 0.98f;
        }

        if (!characterController.isGrounded)
        {
            currentGravity -= gravity * Time.deltaTime;
        }
        else
        {
            currentGravity = 0f;

            if (isOnSlope)
            {
                currentGravity = -2f;
            }
        }

        float normalMoveDelta = 20f;
        float tiredMoveDelta = 14f;
        float runningMoveDelta = 18f;
        float quickStopMoveDelta = 64f;

        Vector3 targetVelocity = ((transform.right * inputVector.x) + (transform.forward * inputVector.y)) * currentMoveSpeed;

        if (wantsToSlide || isSliding)
        {
            wantsToSlide = false;

            float minimumSpeedToSlide = baseMoveSpeed * 2.7f;

            if (isCloseToGround && characterController.velocity.magnitude > minimumSpeedToSlide || isSliding)
            {
                if (shouldSlideBoost)
                {
                    shouldSlideBoost = false;

                    float slideBoostMultiplier = 0.5f;

                    velocity += velocity * slideBoostMultiplier;
                }

                isSliding = true;
                isCrouching = true;

                if (wantsToStopSliding)
                {
                    float stopStrength = 4f;
                    velocity -= velocity * stopStrength * Time.deltaTime;
                }
                else
                {
                    velocity -= velocity * Time.deltaTime;
                }

                float maximumSpeedToStopSlide = baseMoveSpeed * 1.5f;

                if (!isCloseToGround || characterController.velocity.magnitude <= maximumSpeedToStopSlide)
                {
                    isSliding = false;

                    OnPlayerStoppedSliding?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        else
        {
            wantsToSlide = false;
            wantsToStopSliding = false;
            isSliding = false;
            shouldSlideBoost = true;

            if (hasMoveInput)
            {
                if (IsExhausted())
                {
                    velocity = Vector3.MoveTowards(velocity, new Vector3(targetVelocity.x, velocity.y, targetVelocity.z), tiredMoveDelta * Time.deltaTime);
                }
                else
                {
                    if (!isPressingRunKey)
                    {
                        if (velocity.magnitude > baseMoveSpeed)
                        {
                            velocity = Vector3.MoveTowards(velocity, new Vector3(targetVelocity.x, velocity.y, targetVelocity.z), quickStopMoveDelta * Time.deltaTime);
                        }
                        else
                        {
                            velocity = Vector3.MoveTowards(velocity, new Vector3(targetVelocity.x, velocity.y, targetVelocity.z), currentNormalMoveDelta * Time.deltaTime);
                        }
                    }
                    else
                    {
                        velocity = Vector3.SmoothDamp(velocity, Vector3.MoveTowards(velocity, new Vector3(targetVelocity.x, velocity.y, targetVelocity.z), runningMoveDelta * Time.deltaTime), ref smoothDampRunVelocity, currentRunLag * Time.deltaTime);
                    }
                }
            }
            else
            {
                if (velocity.magnitude > 2.5f)
                {
                    velocity = Vector3.MoveTowards(velocity, Vector3.zero, quickStopMoveDelta * Time.deltaTime);
                }
                else
                {
                    velocity = Vector3.MoveTowards(velocity, Vector3.zero, normalMoveDelta * Time.deltaTime);
                }
            }
        }

        if (isCrouching)
        {
            Crouch(false);
        }
        else
        {
            if (!isHeadBlocked)
            {
                Crouch(true);
            }
        }

        velocity.y = currentGravity;

        previousPosition = transform.position;

        Vector3 realVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        Vector3 appliedVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float speedDifference = Mathf.Abs(realVelocity.magnitude - appliedVelocity.magnitude);
        float speedDifferenceMax = 3f;

        if (!isSliding && speedDifference >= speedDifferenceMax)
        {
            velocity = Vector3.zero;
        }

        characterController.Move(velocity * Time.deltaTime);

        isMoving = (transform.position - previousPosition) != Vector3.zero;
    }

    private void UpdateLook()
    {
        cameraPitch += mouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraYaw += mouseDelta.x;

        playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
        transform.eulerAngles = Vector3.up * cameraYaw;
    }

    private void UpdateMovementProperties()
    {
        float carryMass = interaction.GetCarryMass();

        currentNormalMoveDelta = Utils.MapRange(carryMass, 0f, mass, MAX_NORMAL_MOVE_DELTA, MIN_NORMAL_MOVE_DELTA);
        currentRunSharpness = Utils.MapRange(carryMass, 0f, mass, MAX_RUN_SHARPNESS, MIN_RUN_SHARPNESS);
        currentRunLag = Utils.MapRange(carryMass, 0f, mass, MIN_RUN_LAG, MAX_RUN_LAG);
    }

    private void Crouch(bool standUp)
    {
        if (standUp)
        {
            characterController.center = Vector3.up * 1f;
            characterController.height = 2f;
        }
        else
        {
            characterController.center = Vector3.up * 0.5f;
            characterController.height = 1f;
        }
    }

    public float StaminaNormalized()
    {
        return stamina / 100f;
    }

    public bool IsExhausted()
    {
        return exhaustedTimer > 0f;
    }

    public Transform GetPlayerCameraTransform()
    {
        return playerCamera.transform;
    }

    public bool HasMoveInput()
    {
        return hasMoveInput;
    }

    public bool IsPressingRunKey()
    {
        return isPressingRunKey;
    }

    public bool CanRun()
    {
        return canRun;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }

    public bool IsCloseToGround()
    {
        return isCloseToGround;
    }

    public bool IsHeadBlocked()
    {
        return isHeadBlocked;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public bool IsSliding()
    {
        return isSliding;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public float GetMaxVelocityMagnitude()
    {
        return baseMoveSpeed * runSpeedMultiplier;
    }
}
