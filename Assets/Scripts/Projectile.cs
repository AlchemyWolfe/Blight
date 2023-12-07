using MalbersAnimations;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new HashSet<int> { 20, 21, 23 };

    [HideInInspector]
    public float Damage;

    [HideInInspector]
    public float Velocity = 1;

    [HideInInspector]
    public ProjectilePoolSO Pool;

    [HideInInspector]
    public bool InUse;

    public virtual void FixedUpdate()
    {
        var distance = Time.fixedDeltaTime * Velocity;
        transform.position = transform.position + (transform.forward * distance);
    }

    public void ReturnToPool()
    {
        Pool.ReturnProjectile(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherLayer = other.gameObject.layer;
        if (otherLayer != gameObject.layer && ValidLayers.Contains(otherLayer))
        {
            // We've hit something not aligned with us, let's see if we can damage it!
            var damagable = other.GetComponent<IMDamage>();
            if (damagable != null)
            {
                var modifier = new StatModifier() { ID = Pool.Stat, modify = StatOption.SubstractValue, Value = Damage };
                damagable.ReceiveDamage(-transform.forward,
                    this.gameObject,
                    modifier,
                    false,
                    true,
                    null,
                    false,
                    null);
            }
            // Either way, we're dead.
            ReturnToPool();
        }
    }
}
