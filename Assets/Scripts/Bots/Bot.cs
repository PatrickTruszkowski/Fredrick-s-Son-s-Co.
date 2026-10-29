using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public enum BotState
{
    Sleep, Idle, Roam, Intimidate, Chase, SpecialKill,
}

[RequireComponent(typeof(NavMeshAgent))]
public class Bot : NetworkBehaviour
{
    public event EventHandler OnRagdoll;

    [SerializeField] protected float maximumSightDistance = 20f;
    [SerializeField][Range(0f, 1f)] protected float maximumSightAngle = 0.25f;

    protected BotState botState = BotState.Idle;
    protected List<Transform> playerTransformList;
    protected Transform currentTargetPlayerTransform;

    private NavMeshAgent navMeshAgent = null;

    protected virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        playerTransformList = new List<Transform>();

        Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        Player player = sender as Player;

        playerTransformList.Add(player.transform);
    }

    protected void MoveToLocation(Vector3 location, float speed, float angularSpeed, float acceleration)
    {
        navMeshAgent.speed = speed;
        navMeshAgent.angularSpeed = angularSpeed;
        navMeshAgent.acceleration = acceleration;
        navMeshAgent.SetDestination(location);
    }

    protected Vector3 ChooseRandomValidPosition(float min, float max)
    {
        Vector3 randomPosition = Vector3.zero;
        bool isValidPosition = false;

        do
        {
            randomPosition = new Vector3(UnityEngine.Random.Range(min, max), 0f, UnityEngine.Random.Range(min, max));

            float smallOffset = 0.1f;
            isValidPosition = !Physics.CheckSphere(randomPosition + Vector3.up * (navMeshAgent.radius + smallOffset), navMeshAgent.radius);

        } while (!isValidPosition);

        return randomPosition;
    }

    protected Transform GetClosestPlayerTransform()
    {
        if (playerTransformList.Count == 0)
        {
            return null;
        }

        if (playerTransformList.Count == 1)
        {
            return playerTransformList[0];
        }

        float closestDistance = Vector3.Distance(transform.position, playerTransformList[0].position);
        int closestPlayerIndex = 0;

        for (int i = 1; i < playerTransformList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, playerTransformList[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayerIndex = i;
            }
        }

        return playerTransformList[closestPlayerIndex];
    }

    protected Vector3 GetDirectionToClosestPlayer()
    {
        Transform closestPlayer = GetClosestPlayerTransform();

        if (closestPlayer == null)
        {
            return Vector3.zero;
        }

        return (closestPlayer.position - transform.position).normalized;
    }

    protected Vector3 GetDirectionToPlayer(Transform playerTransform)
    {
        return (playerTransform.position - transform.position).normalized;
    }

    protected float GetDistanceFromClosestPlayer()
    {
        return Vector3.Distance(GetClosestPlayerTransform().position, transform.position);
    }

    protected float GetDistanceFromPlayer(Transform playerTransform)
    {
        return Vector3.Distance(playerTransform.position, transform.position);
    }

    protected void SetCurrentTargetPlayerTransform(Transform target)
    {
        currentTargetPlayerTransform = target;
    }

    public bool IsTargetPlayerVisible()
    {
        if (currentTargetPlayerTransform == null)
        {
            return false;
        }

        if (Physics.Raycast(transform.position + Vector3.up, GetDirectionToPlayer(currentTargetPlayerTransform), out RaycastHit hitData, maximumSightDistance))
        {
            if (hitData.transform.TryGetComponent(out Player _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool IsClosestPlayerVisible()
    {
        if (playerTransformList.Count == 0)
        {
            return false;
        }

        if (Physics.Raycast(transform.position + Vector3.up, GetDirectionToClosestPlayer(), out RaycastHit hitData, maximumSightDistance))
        {
            if (hitData.transform.TryGetComponent(out Player _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool IsAnyPlayerLooking()
    {
        if (playerTransformList.Count == 0)
        {
            return false;
        }

        foreach (Transform playerTransform in playerTransformList)
        {
            float dotProduct = Vector3.Dot(playerTransform.transform.forward, GetDirectionToPlayer(playerTransform));

            if (dotProduct >= 0f)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsLookingAtClosestPlayer()
    {
        if (playerTransformList.Count == 0)
        {
            return false;
        }

        return Vector3.Dot(GetDirectionToClosestPlayer(), transform.forward) >= 0f;
    }

    public bool IsTargetPlayerLooking()
    {
        return Vector3.Dot(currentTargetPlayerTransform.transform.forward, GetDirectionToPlayer(currentTargetPlayerTransform)) >= 0f;
    }

    public bool IsLookingAtTargetPlayer()
    {
        if (currentTargetPlayerTransform == null)
        {
            return false;
        }

        return Vector3.Dot(GetDirectionToPlayer(currentTargetPlayerTransform), transform.forward) >= maximumSightAngle;
    }

    public void Ragdoll()
    {
        RagdollServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RagdollServerRpc()
    {
        RagdollClientRpc();
    }

    [ClientRpc]
    private void RagdollClientRpc()
    {
        OnRagdoll?.Invoke(this, EventArgs.Empty);
        enabled = false;
    }

    private void OnDisable()
    {
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }
}
