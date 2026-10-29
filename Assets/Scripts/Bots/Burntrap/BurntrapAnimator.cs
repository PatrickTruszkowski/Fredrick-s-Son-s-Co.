using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BurntrapAnimator : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BurntrapAI burntrapAI = null;
    [SerializeField] private Rig headRig = null;
    [SerializeField] private GameObject breathingAudio = null;

    private Animator animator = null;
    private float animatorSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        burntrapAI.OnBotStateChanged += BurntrapAI_OnBotStateChanged;
        burntrapAI.OnRagdoll += BurntrapAI_OnRagdoll;
    }

    private void BurntrapAI_OnRagdoll(object sender, System.EventArgs e)
    {
        animator.enabled = false;
        breathingAudio.SetActive(false);
    }

    private void BurntrapAI_OnBotStateChanged(object sender, BurntrapAI.OnBotStateChangedEventArgs e)
    {
        switch (e.botState)
        {
            case BotState.Idle:

                animatorSpeed = 1f;

                break;
            case BotState.Roam:

                animatorSpeed = 1.25f;

                break;
            case BotState.Intimidate:

                animatorSpeed = 0.75f;

                break;
            case BotState.Chase:

                animatorSpeed = 3f;

                break;
            case BotState.SpecialKill:

                animatorSpeed = 10f;

                break;
        }
    }

    private void Update()
    {
        SetAnimator();
        UpdateHeadLook();
    }
    
    private void UpdateHeadLook()
    {
        if (burntrapAI.IsLookingAtTargetPlayer())
        {
            SetRigWeight(1f, 16f);
        }
        else
        {
            SetRigWeight(0f, 16f);
        }
    }

    private void SetRigWeight(float weight, float changeSpeed)
    {
        headRig.weight = Mathf.Lerp(headRig.weight, weight, Time.deltaTime * changeSpeed);
    }

    private void SetAnimator()
    {
        animator.SetBool("Is Roaming", burntrapAI.IsMoving());
        animator.speed = animatorSpeed;
    }
}
