using MalbersAnimations;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new() { 20, 21, 23 };

    [Header("In Game Values")]
    public ProjectilePoolSO Pool;
    public bool InUse;

    public float Velocity;
    public float Damage = 1f;
    public float Size = 0.2f;
    public GameObject Attacker;

    [Header("Definition Values")]
    public bool IsPiercing;
    private float MaxLifespan = 30f;

    private float Lifespan;
    private int _level;
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            SetLevelValues();
        }
    }

    public virtual void FixedUpdate()
    {
        // TODO: Pause Game ability
        var distance = Time.fixedDeltaTime * Velocity;
        transform.position = transform.position + (transform.forward * distance);
        Lifespan -= distance;
        if (Lifespan <= 0f)
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

    // Adjust Damage, Size, Lifespan, and anything else based on projectile level.
    public virtual void SetLevelValues()
    {
        Damage = 1f;
        Size = 0.2f;
    }

    // Do anything necessary after values have been set.
    public virtual void Initialize()
    {
        Lifespan = MaxLifespan;
        gameObject.transform.localScale = new Vector3(Size, Size, Size);
    }

    public virtual void ReturnToPool()
    {
        Pool.ReturnProjectile(this);
    }

    public virtual void InflictDamage(IMDamage damagable)
    {
        var modifier = new StatModifier() { ID = Pool.Stat, modify = StatOption.SubstractValue, Value = Damage };
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
        if (!IsPiercing)
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
