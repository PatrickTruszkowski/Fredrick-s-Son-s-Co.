using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

public class PlayerArmsAnimator : NetworkBehaviour
{
    [SerializeField] private Animator leftArmAnimator;
    [SerializeField] private Animator rightArmAnimator;
    [SerializeField] private Animator rightHandAnimator;
    [SerializeField] private Animator leftHandAnimator;
    [SerializeField] private Player player;
    [SerializeField] private Interaction interaction;
    [SerializeField] private HealthSystem playerHealthSystem;
    [SerializeField] private LayerMask layerMask;

    // Movement
    private const string IS_MOVING = "IsMoving";
    private const string HAS_MOVE_INPUT = "HasMoveInput";
    private const string IS_RUNNING = "IsRunning";
    private const string IS_SLIDING = "IsSliding";
    private const string INTERACT = "Interact";
    private const string INTERACT_READY = "InteractReady";
    private const string MIDDLE_FINGER = "MiddleFinger";
    private const string THROW_READY = "ThrowReady";
    private const string THROW = "Throw";
    private const string DEATH = "Death";
    private const string HOLD = "Hold";
    private const string LIFT = "Lift";
    private const string START_SWITCH = "StartSwitch";
    private const string IS_SWITCHING = "IsSwitching";
    
    // Products
    private const string FLASHLIGHT_HOLD = "FlashlightHold";
    private const string FLASHLIGHT_USE = "FlashlightUse";
    private const string SHOTGUN_HOLD = "ShotgunHold";
    private const string SHOTGUN_RECOIL = "ShotgunRecoil";
    private const string FORBIDDEN_PINEAPPLE_HOLD = "ForbiddenPineappleHold";
    private const string FORBIDDEN_PINEAPPLE_USE = "ForbiddenPineappleUse";

    private string holdAnimationName = HOLD;
    private string useAnimationName;
    private IUseable heldUseableProduct;
    private bool isShowingMiddleFinger = false;
    private bool isHandIntersecting = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        interaction.OnProductTaken += Interaction_OnProductTaken;
        interaction.OnProductBeganSwitching += Interaction_OnProductBeganSwitching;
        interaction.OnProductFinishedSwitching += Interaction_OnProductFinishedSwitching;
        interaction.OnProductDropped += Interaction_OnProductDropped;
        interaction.OnValidInteract += Interaction_OnValidInteract;
        interaction.OnThrowReady += Interaction_OnThrowReady;
        interaction.OnProductThrown += Interaction_OnProductThrown;

        InputManager.Singleton.OnEmote0 += InputManager_OnEmote0;

        playerHealthSystem.OnDeath += PlayerHealthSystem_OnDeath;
    }

    private void PlayerHealthSystem_OnDeath(object sender, System.EventArgs e)
    {
        leftArmAnimator.SetBool(DEATH, true);
        rightArmAnimator.SetBool(DEATH, true);
    }

    private void Interaction_OnProductThrown(object sender, System.EventArgs e)
    {
        rightArmAnimator.SetTrigger(THROW);

        leftArmAnimator.SetBool(THROW_READY, false);
        rightArmAnimator.SetBool(THROW_READY, false);
        leftHandAnimator.SetBool(THROW_READY, false);
    }

    private void Interaction_OnThrowReady(object sender, System.EventArgs e)
    {
        leftArmAnimator.SetBool(THROW_READY, true);
        rightArmAnimator.SetBool(THROW_READY, true);
        leftHandAnimator.SetBool(THROW_READY, true);
    }

    private void InputManager_OnEmote0(object sender, System.EventArgs e)
    {
        isShowingMiddleFinger = !isShowingMiddleFinger;

        leftArmAnimator.SetBool(MIDDLE_FINGER, isShowingMiddleFinger);
        leftHandAnimator.SetBool(MIDDLE_FINGER, isShowingMiddleFinger);
    }

    private void Interaction_OnValidInteract(object sender, System.EventArgs e)
    {
        leftArmAnimator.SetTrigger(INTERACT);
        leftHandAnimator.SetTrigger(INTERACT);
    }

    private void Interaction_OnProductDropped(object sender, System.EventArgs e)
    {
        if (heldUseableProduct != null)
        {
            heldUseableProduct.OnUsed -= HeldUseableProduct_OnUsed;
        }

        rightArmAnimator.SetBool(HOLD, false);
        rightHandAnimator.SetBool(HOLD, false);

        rightArmAnimator.SetBool(THROW_READY, false);
        leftArmAnimator.SetBool(THROW_READY, false);
        leftHandAnimator.SetBool(THROW_READY, false);

        if (!holdAnimationName.IsNullOrEmpty())
        {
            rightHandAnimator.SetBool(holdAnimationName, false);
        }
    }

    private void Interaction_OnProductTaken(object sender, System.EventArgs e)
    {
        UpdateHeldProductAnimation();
    }

    private void Interaction_OnProductBeganSwitching(object sender, System.EventArgs e)
    {
        rightArmAnimator.SetBool(THROW_READY, false);
        leftArmAnimator.SetBool(THROW_READY, false);
        leftHandAnimator.SetBool(THROW_READY, false);

        rightArmAnimator.SetTrigger(START_SWITCH);
        rightArmAnimator.SetBool(IS_SWITCHING, true);
    }

    private void Interaction_OnProductFinishedSwitching(object sender, System.EventArgs e)
    {
        rightArmAnimator.SetBool(IS_SWITCHING, false);

        if (!holdAnimationName.IsNullOrEmpty())
        {
            rightHandAnimator.SetBool(holdAnimationName, false);
        }

        UpdateHeldProductAnimation();
    }

    private void HeldUseableProduct_OnUsed(object sender, System.EventArgs e)
    {
        if (!useAnimationName.IsNullOrEmpty())
        {
            if (AnimationParameterExists(rightHandAnimator, useAnimationName)) rightHandAnimator.SetTrigger(useAnimationName);
            if (AnimationParameterExists(rightArmAnimator, useAnimationName)) rightArmAnimator.SetTrigger(useAnimationName);
        }
    }

    private void Update()
    {
        CheckForHandIntersection();
        UpdateAnimations();
        SyncArmAnimation();
    }

    private void CheckForHandIntersection()
    {
        if (!interaction.IsHoldingProduct())
        {
            isHandIntersecting = false;
            return;
        }

        float radius = 0.05f;
        float xOffset = 0.35f;
        float yOffset = 1.4f;
        float zOffset = 0.5f;
        Collider[] colliders = new Collider[1];
        Vector3 position = player.transform.position + player.transform.right * xOffset + player.transform.up * yOffset + player.transform.forward * zOffset;

        int colliderCount = Physics.OverlapSphereNonAlloc(position, radius, colliders, layerMask, QueryTriggerInteraction.Ignore);

        isHandIntersecting = colliderCount > 0;
    }

    private void UpdateHeldProductAnimation()
    {
        heldUseableProduct = interaction.GetHeldProduct() as IUseable;

        if (heldUseableProduct != null)
        {
            heldUseableProduct.OnUsed += HeldUseableProduct_OnUsed;
        }

        rightArmAnimator.SetBool(HOLD, true);
        rightHandAnimator.SetBool(HOLD, true);

        holdAnimationName = heldUseableProduct switch
        {
            Flashlight => FLASHLIGHT_HOLD,
            Shotgun => SHOTGUN_HOLD,
            ForbiddenPineapple => FORBIDDEN_PINEAPPLE_HOLD,
            _ => ""
        };

        if (!holdAnimationName.IsNullOrEmpty())
        {
            rightHandAnimator.SetBool(holdAnimationName, true);
        }

        useAnimationName = heldUseableProduct switch
        {
            Flashlight => FLASHLIGHT_USE,
            Shotgun => SHOTGUN_RECOIL,
            ForbiddenPineapple => FORBIDDEN_PINEAPPLE_USE,
            _ => ""
        };
    }

    private void UpdateAnimations()
    {
        if (!IsOwner)
        {
            return;
        }

        bool isMoving = player.IsMoving();
        bool isPressingRunKey = player.IsPressingRunKey();
        bool isSliding = player.IsSliding();
        bool canRun = player.CanRun();
        bool hasMoveInput = player.HasMoveInput();
        bool isCloseToGround = player.IsCloseToGround();
        bool isCrouching = player.IsCrouching();

        rightArmAnimator.SetBool(LIFT, isHandIntersecting);

        leftArmAnimator.SetBool(IS_MOVING, isMoving);
        rightArmAnimator.SetBool(IS_MOVING, isMoving);

        leftArmAnimator.SetBool(IS_RUNNING, !isSliding && isPressingRunKey && canRun && isMoving && hasMoveInput);
        rightArmAnimator.SetBool(IS_RUNNING, !isSliding && isPressingRunKey && canRun && isMoving && hasMoveInput);

        leftArmAnimator.SetBool(HAS_MOVE_INPUT, hasMoveInput);
        rightArmAnimator.SetBool(HAS_MOVE_INPUT, hasMoveInput);

        leftArmAnimator.SetBool(IS_SLIDING, isSliding);
        rightArmAnimator.SetBool(IS_SLIDING, isSliding);

        bool isInteractReady = interaction.IsCloseToInteractReadyInteractable();

        leftArmAnimator.SetBool(INTERACT_READY, isInteractReady);
        leftHandAnimator.SetBool(INTERACT_READY, isInteractReady);

        leftHandAnimator.SetBool(IS_MOVING, isMoving);
        rightHandAnimator.SetBool(IS_MOVING, isMoving);

        leftHandAnimator.SetBool(IS_RUNNING, isPressingRunKey && canRun && isMoving && hasMoveInput);
        rightHandAnimator.SetBool(IS_RUNNING, isPressingRunKey && canRun && isMoving && hasMoveInput);

        leftHandAnimator.SetBool(HAS_MOVE_INPUT, hasMoveInput);
        rightHandAnimator.SetBool(HAS_MOVE_INPUT, hasMoveInput);

        float minimumAnimationSpeed = 0.6f;
        float maximumAnimationSpeed = 1.3f;

        if (isCloseToGround && !isCrouching && isMoving && isPressingRunKey && canRun)
        {
            float maximumVelocity = 5.8f;

            leftArmAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / maximumVelocity, minimumAnimationSpeed, maximumAnimationSpeed);
            rightArmAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / maximumVelocity, minimumAnimationSpeed, maximumAnimationSpeed);
        }
        else if (isCloseToGround && !isCrouching && isMoving)
        {
            leftArmAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / 2.1f, minimumAnimationSpeed, maximumAnimationSpeed);
            rightArmAnimator.speed = Mathf.Clamp(player.GetVelocity().magnitude / 2.1f, minimumAnimationSpeed, maximumAnimationSpeed);
        }
        else
        {
            leftArmAnimator.speed = 1f;
            rightArmAnimator.speed = 1f;
        }
    }

    private void SyncArmAnimation()
    {
        if (leftArmAnimator.IsInTransition(0) || rightArmAnimator.IsInTransition(0))
        {
            return;
        }

        AnimatorStateInfo rightArmAnimatorStateInfo = rightArmAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo leftArmAnimatorStateInfo = leftArmAnimator.GetCurrentAnimatorStateInfo(0);

        if (rightArmAnimatorStateInfo.fullPathHash != leftArmAnimatorStateInfo.fullPathHash)
        {
            return;
        }

        float rightArmAnimatorStateInfoTime = rightArmAnimatorStateInfo.normalizedTime % 1f;
        float leftArmAnimatorStateInfoTime = leftArmAnimatorStateInfo.normalizedTime % 1f;

        if (Mathf.Abs(rightArmAnimatorStateInfoTime - leftArmAnimatorStateInfoTime) > 0f)
        {
            leftArmAnimator.Play(rightArmAnimatorStateInfo.fullPathHash, 0, rightArmAnimatorStateInfoTime);
            leftArmAnimator.Update(0f);
        }
    }

    private bool AnimationParameterExists(Animator animator, string parameterName)
    {
        for (int i = 0; i < animator.parameters.Length; i++)
        {
            if (animator.parameters[i].name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
