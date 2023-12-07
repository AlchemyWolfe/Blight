using MalbersAnimations;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new HashSet<int> { 20, 21, 23 };

    public float Damage;
    public float Velocity = 1;
    public GameObject Attacker;
    public ProjectilePoolSO Pool;
    public bool InUse;
    public float Lifespan;

    public virtual void FixedUpdate()
    {
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

    public void Reset()
    {
        Lifespan = Pool.Lifespan;
    }

    public void ReturnToPool()
    {
        Pool.ReturnProjectile(this);
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
        if (otherRigidBody != null)
        {
            return;
        }
        var baseObject = otherRigidBody.gameObject;
        var damagable = baseObject.GetComponent<IMDamage>();
        if (damagable != null)
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
        // Either way, we're dead.
        Splash();
    }
}
