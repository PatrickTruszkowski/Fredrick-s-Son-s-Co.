using System;
using Unity.Netcode;
using UnityEngine;

public class BurntrapAI : Bot
{
    public event EventHandler<OnBotStateChangedEventArgs> OnBotStateChanged;
    public class OnBotStateChangedEventArgs : EventArgs
    {
        public BotState botState;
    }

    [SerializeField] private Transform[] predefinedPositionArray = null;

    [Header("AI Settings")]
    [SerializeField] private float idleTimerMax = 10f;
    [SerializeField] private float idleTimerMin = 3f;
    [Space]
    [SerializeField] private float destinationTimer = 20f;
    [Space]
    [SerializeField] private float intimidateTimerMax = 8f;
    [SerializeField] private float intimidateTimerMin = 5f;
    [Space]
    [SerializeField] private float killDistance = 3f;

    [Header("Debug")]
    [SerializeField] private bool useDebug = true;
    [SerializeField] private bool idle = false;

    private Vector3 randomPosition = Vector3.zero;
    private Vector3 rayOrigin = Vector3.zero;
    private Vector3 lastSeenPlayerPosition = Vector3.zero;
    private Vector3 previousPosition = Vector3.zero;

    private float idleTimer = 0f;
    private float idleTimerCurrent = 0f;
    private float destinationTimerCurrent = 0f;
    private float intimidateTimer = 0f;
    private float intimidateTimerCurrent = 0f;
    private float chance = 0f;

    private bool isMoving = false;

    protected override void Awake()
    {
        base.Awake();

        if (!IsServer)
        {
            return;
        }

        previousPosition = transform.position;

        idleTimer = UnityEngine.Random.Range(idleTimerMin, idleTimerMax);

        InitializeClientRpc(idleTimer);
    }

    [ClientRpc]
    private void InitializeClientRpc(float idleTimer)
    {
        this.idleTimer = idleTimer;

        botState = BotState.Idle;

        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });
    }

    protected override void Start()
    {
        base.Start();

        CholeraFreddy.Singleton.OnFreddyShot += CholeraFreddy_OnFreddyShot;
    }

    private void CholeraFreddy_OnFreddyShot(object sender, EventArgs e)
    {
        botState = BotState.SpecialKill;
    }

    private void Update()
    {
        if (idle)
        {
            return;
        }

        if (!IsServer)
        {
            return;
        }

        isMoving = transform.position != previousPosition;

        previousPosition = transform.position;

        RunPatrolStates();
        DetermineKill();

        if (useDebug)
        {
            DrawDebug();
        }
    }

    private void RunPatrolStates()
    {
        switch (botState)
        {
            case BotState.Idle:

                Idle();

                break;
            case BotState.Roam:

                Roam();

                break;
            case BotState.Intimidate:

                Intimidate();

                break;
            case BotState.Chase:

                Chase();

                break;
            case BotState.SpecialKill:

                SpecialKill();

                break;
        }
    }

    private void SpecialKill()
    {
        SpecialKillClientRpc();
    }

    [ClientRpc]
    private void SpecialKillClientRpc()
    {
        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });

        MoveToLocation(currentTargetPlayerTransform.position, 25f, 250f, 250f);
    }

    private void DetermineKill()
    {
        if (currentTargetPlayerTransform == null)
        {
            return;
        }

        // Refactor this to be more reliable than just using the distance, so use SphereCast or similar.
        if ((GetDistanceFromPlayer(currentTargetPlayerTransform) <= killDistance) && IsTargetPlayerVisible() && IsLookingAtTargetPlayer())
        {
            // Access the player contacting and call an Attack function or something like that.
            // This is also where we will call a client rpc to hurt the target.

            Debug.LogError("BurntrapAI.DetermineKill()");
        }
    }

    private void Idle()
    {
        if ((currentTargetPlayerTransform != null && IsLookingAtTargetPlayer() && IsTargetPlayerVisible()) || (IsLookingAtClosestPlayer() && IsClosestPlayerVisible()))
        {
            TransitionToIntimidateState();
        }
        else
        {
            if (idleTimerCurrent < idleTimer)
            {
                idleTimerCurrent += Time.deltaTime;
                idleTimerCurrent = Mathf.Clamp(idleTimerCurrent, 0f, idleTimer);
            }
            else
            {
                idleTimerCurrent = 0f;

                chance = UnityEngine.Random.Range(0f, 100f);

                TransitionToRoamState();
            }
        }
    }

    private void Roam()
    {
        Debug.DrawLine(transform.position, randomPosition, Color.green);

        RoamClientRpc();

        if (IsLookingAtClosestPlayer() && IsClosestPlayerVisible())
        {
            TransitionToIntimidateState();
        }

        if (destinationTimerCurrent < destinationTimer)
        {
            destinationTimerCurrent += Time.deltaTime;
            destinationTimerCurrent = Mathf.Clamp(destinationTimerCurrent, 0f, destinationTimer);
        }
        else
        {
            destinationTimerCurrent = 0f;
            TransitionToIdleState();
        }

        if (!isMoving && destinationTimerCurrent > 1f)
        {
            destinationTimerCurrent = 0f;
            TransitionToIdleState();
        }
    }

    [ClientRpc]
    private void RoamClientRpc()
    {
        MoveToLocation(randomPosition, 2.5f, 120f, 6f);
    }

    private void Intimidate()
    {
        IntimidateClientRpc();

        if (intimidateTimerCurrent < intimidateTimer)
        {
            intimidateTimerCurrent += Time.deltaTime;
            intimidateTimerCurrent = Mathf.Clamp(intimidateTimerCurrent, 0f, intimidateTimer);
        }
        else
        {
            if (IsTargetPlayerVisible())
            {
                intimidateTimerCurrent = 0f;
                TransitionToChaseState();
            }
            else
            {
                intimidateTimerCurrent = 0f;
                TransitionToIdleState();
            }
        }
    }

    [ClientRpc]
    private void IntimidateClientRpc()
    {
        if (IsTargetPlayerVisible())
        {
            lastSeenPlayerPosition = currentTargetPlayerTransform.position;
        }

        MoveToLocation(lastSeenPlayerPosition, 1f, 100f, 4f);
    }

    private void Chase()
    {
        ChaseClientRpc();

        float distanceToLastSightPosition = Vector3.Distance(lastSeenPlayerPosition, transform.position);

        if (distanceToLastSightPosition < 1f && !IsTargetPlayerVisible())
        {
            TransitionToIdleState();
        }

        if (currentTargetPlayerTransform != null && !IsTargetPlayerVisible() && GetDistanceFromPlayer(currentTargetPlayerTransform) > 10f)
        {
            TransitionToIdleState();
        }
    }

    [ClientRpc]
    private void ChaseClientRpc()
    {
        if (IsTargetPlayerVisible())
        {
            lastSeenPlayerPosition = currentTargetPlayerTransform.position;
        }

        MoveToLocation(lastSeenPlayerPosition, 6f, 180f, 16f);
    }

    private void DrawDebug()
    {
        if (currentTargetPlayerTransform == null)
        {
            return;
        }

        rayOrigin = transform.position + Vector3.up;

        Debug.DrawRay(rayOrigin, GetDirectionToPlayer(currentTargetPlayerTransform) * GetDistanceFromPlayer(currentTargetPlayerTransform), Color.red);
    }

    private void TransitionToIdleState()
    {
        idleTimer = UnityEngine.Random.Range(idleTimerMin, idleTimerMax);
        
        TransitionToIdleStateClientRpc(idleTimer);
    }

    [ClientRpc]
    private void TransitionToIdleStateClientRpc(float idleTimer)
    {
        this.idleTimer = idleTimer;
        botState = BotState.Idle;
        SetCurrentTargetPlayerTransform(null);

        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });
    }

    private void TransitionToRoamState()
    {
        if (chance >= 0f && chance <= 25f)
        {
            randomPosition = predefinedPositionArray[UnityEngine.Random.Range(0, predefinedPositionArray.Length)].position;
        }
        else if (chance > 25f && chance <= 100f)
        {
            randomPosition = ChooseRandomValidPosition(-20f, 20f);
        }

        TransitionToRoamStateClientRpc(randomPosition);
    }

    [ClientRpc]
    private void TransitionToRoamStateClientRpc(Vector3 randomPosition)
    {
        this.randomPosition = randomPosition;
        botState = BotState.Roam;

        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });
    }

    private void TransitionToIntimidateState()
    {
        intimidateTimer = UnityEngine.Random.Range(intimidateTimerMin, intimidateTimerMax);
        
        TransitionToIntimidateStateClientRpc(intimidateTimer);
    }

    [ClientRpc]
    private void TransitionToIntimidateStateClientRpc(float intimidateTimer)
    {
        this.intimidateTimer = intimidateTimer;
        botState = BotState.Intimidate;

        SetCurrentTargetPlayerTransform(GetClosestPlayerTransform());

        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });
    }

    private void TransitionToChaseState()
    {
        TransitionToChaseStateClientRpc();
    }

    [ClientRpc]
    private void TransitionToChaseStateClientRpc()
    {
        botState = BotState.Chase;

        OnBotStateChanged?.Invoke(this, new OnBotStateChangedEventArgs { botState = botState });
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
