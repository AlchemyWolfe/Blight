using MalbersAnimations;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ProjectileParams
{
    public float Damage;
    public float Size;
    public float SizeY;
    public float Lifespan;
    public bool IsPiercing;
    public bool IsTracking;
    public float TurnSpeed;
}

[Serializable]
public struct ProjectleUpgradeParams
{
    public float Damage;
    public float Size;
}

public class Projectile : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new() { 20, 21, 23 };

    [Header("In Game Values")]
    public ProjectilePoolSO Pool;
    public ProjectileParams Params;
    public bool InUse;
    public GameObject Attacker;

    private float RemainingLifespan;
    private float Velocity;

    public virtual void FixedUpdate()
    {
        // TODO: Pause Game ability
        var distance = Time.fixedDeltaTime * Velocity;
        transform.position = transform.position + (transform.forward * distance);
        RemainingLifespan -= distance;
        if (RemainingLifespan <= 0f)
        {
            Fizzle();
        }

    }

    // Do anything that happens when we reach the end of our lifespan.
    public virtual void Fizzle()
    {
        ReturnToPool();
    }

    // Do anything that happens when we impact into something.
    public virtual void Splash()
    {
        ReturnToPool();
    }

    // Do anything necessary after values have been set.
    public virtual void Initialize(Vector3 forward, ProjectileParams projectileParams)
    {
        transform.forward = forward == Vector3.zero ? Vector3.forward : forward.normalized;
        Velocity = forward.magnitude;
        Params = projectileParams;
        RemainingLifespan = Params.Lifespan;
        gameObject.transform.localScale = new Vector3(Params.Size, Params.SizeY, Params.Size);
    }

    public virtual void ReturnToPool()
    {
        Pool.ReturnProjectile(this);
    }

    public virtual void InflictDamage(IMDamage damagable)
    {
        var modifier = new StatModifier() { ID = Pool.Stat, modify = StatOption.SubstractValue, Value = Params.Damage };
        damagable.ReceiveDamage(-transform.forward,
            Attacker,
            modifier,
            false,
            true,
            null,
            false,
            null);
    }

    public virtual void OnImpact(Collider other)
    {
        if (!Params.IsPiercing)
        {
            // Since we hit something, we splash.
            Splash();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherLayer = other.gameObject.layer;
        if (otherLayer == gameObject.layer || !ValidLayers.Contains(otherLayer))
        {
            return;
        }

        // We've hit something not aligned with us, let's see if we can damage it!
        var otherRigidBody = other.attachedRigidbody;
        if (otherRigidBody == null)
        {
            return;
        }
        var baseObject = otherRigidBody.gameObject;
        if (baseObject.TryGetComponent<IMDamage>(out var damagable))
        {
            InflictDamage(damagable);
        }
    }
}
