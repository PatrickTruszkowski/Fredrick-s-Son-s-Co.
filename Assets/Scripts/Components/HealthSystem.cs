using System;
using UnityEngine;

public enum DamageStrength
{
    Normal = 1, Heavy = 2, Deadly = 3
}

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnDeath;
    public event EventHandler OnHitCountChanged;
    public event EventHandler OnDamaged;

    private const float HIT_COUNT_REGENERATION_TIME_FROM_HEAVY = 4f;
    private const float HIT_COUNT_REGENERATION_TIME_FROM_NORMAL = 6f;

    private int hitCount = 0;
    private float hitRegenerationTimer = 0f;

    private void Update()
    {
        if (IsDead())
        {
            return;
        }

        if (transform.position.y <= -1000f)
        {
            Damage(DamageStrength.Deadly);
            return;
        }

        if (hitCount == 0)
        {
            return;
        }

        // First check if we're damaged heavily.
        if (hitCount > 1)
        {
            if (hitRegenerationTimer < HIT_COUNT_REGENERATION_TIME_FROM_HEAVY)
            {
                hitRegenerationTimer += Time.deltaTime;

                return;
            }
            else
            {
                hitRegenerationTimer = 0f;
                hitCount--;

                OnHitCountChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // We're 100% damaged normally if we get to this point.
        if (hitRegenerationTimer < HIT_COUNT_REGENERATION_TIME_FROM_NORMAL)
        {
            hitRegenerationTimer += Time.deltaTime;
        }
        else
        {
            hitRegenerationTimer = 0f;
            hitCount--;

            OnHitCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Damage(DamageStrength damageStrength)
    {
        if (IsDead())
        {
            return;
        }

        hitCount += (int) damageStrength;

        OnHitCountChanged?.Invoke(this, EventArgs.Empty);
        OnDamaged?.Invoke(this, EventArgs.Empty);

        if (IsDead())
        {
            Die();
            return;
        }

        hitRegenerationTimer = 0f;
    }

    private void Die()
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDead()
    {
        return hitCount > 2;
    }

    public DamageStrength GetCurrentDamageStrength()
    {
        return (DamageStrength) hitCount;
    }
}
