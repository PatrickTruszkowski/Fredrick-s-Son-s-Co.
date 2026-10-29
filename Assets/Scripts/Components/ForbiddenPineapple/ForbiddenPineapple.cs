using System;
using UnityEngine;

public class ForbiddenPineapple : Product, IUseable
{
    public event EventHandler OnUsed;
    public event EventHandler OnExploded;

    [SerializeField] private Transform pinPrefab;
    [SerializeField] private Transform pinSpawnPointTransform;

    private const float TIME_TO_EXPLODE_MAX = 5f;

    private bool isPinPulled = false;
    private float currentTimeToExplode = 0f;

    protected override void Update()
    {
        base.Update();

        if (isPinPulled)
        {
            currentTimeToExplode += Time.deltaTime;

            if (currentTimeToExplode > TIME_TO_EXPLODE_MAX)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        float blastRadius = 10f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out HealthSystem healthSystem))
            {
                Vector3 targetPosition = hitCollider.bounds.center;

                if (Physics.Linecast(transform.position, targetPosition, out RaycastHit raycastHit, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hitCollider == raycastHit.collider)
                    {
                        float distance = Vector3.Distance(transform.position, targetPosition);
                        float halfDistance = blastRadius * 0.5f;

                        if (distance >= halfDistance)
                        {
                            healthSystem.Damage(DamageStrength.Normal);
                        }
                        else if (distance < halfDistance)
                        {
                            healthSystem.Damage(DamageStrength.Heavy);
                        }
                    }
                }
            }

            if (hitCollider.TryGetComponent(out Breakable breakable))
            {
                Vector3 targetPosition = hitCollider.bounds.center;

                if (Physics.Linecast(transform.position, targetPosition, out RaycastHit raycastHit, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hitCollider == raycastHit.collider)
                    {
                        float distance = Vector3.Distance(transform.position, targetPosition);
                        float halfDistance = blastRadius * 0.5f;

                        if (distance >= halfDistance)
                        {
                            breakable.Break(transform.position, 8f);
                        }
                        else if (distance < halfDistance)
                        {
                            breakable.Break(transform.position, 8f);
                        }
                    }
                }
            }
        }

        if (IsInPocket())
        {
            GetInteraction().GetComponent<HealthSystem>().Damage(DamageStrength.Heavy);
        }

        OnExploded?.Invoke(this, EventArgs.Empty);

        Delete();
    }

    public bool IsProductOn()
    {
        return isPinPulled;
    }

    public void Use()
    {
        if (isPinPulled)
        {
            return;
        }

        isPinPulled = true;

        Transform pinTransform = Instantiate(pinPrefab, pinSpawnPointTransform.position, pinSpawnPointTransform.rotation);
        ObjectPoolManager.Singleton.AddPiece(pinTransform.gameObject);

        Rigidbody pinRigidbody = pinTransform.GetComponent<Rigidbody>();

        float angularVelocity = -16f;
        float velocityY = 1.5f;
        float velocityZ = 2f;

        pinRigidbody.angularVelocity = pinTransform.right * angularVelocity;

        Vector3 playerVelocity = GetInteraction().GetComponent<CharacterController>().velocity;

        pinRigidbody.velocity = playerVelocity + pinTransform.up * velocityY + pinTransform.forward * velocityZ;

        Destroy(pinSpawnPointTransform.gameObject);

        OnUsed?.Invoke(this, EventArgs.Empty);
    }
}
