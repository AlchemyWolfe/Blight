using MalbersAnimations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct ProjectileParams
{
    public float Damage;
    public float Size;
    public float SizeY;
    public float Lifespan;
    public bool IsPiercing;
    public bool IsTracking;
    public GameObject TargetContainer;
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

    public GameObject ProjectileGO;

    [Header("In Game Values")]
    public GameSceneToolsSO Tools;
    public ProjectilePoolSO Pool;
    public ProjectileParams Params;
    public bool InUse;
    public GameObject Attacker;
    public bool Spin;
    public Vector3 SpinSpeed;

    private float RemainingLifespan;
    private float Velocity;
    private MeshRenderer ProjectileMeshRenderer;
    private BlightCreature TrackTarget;
    private Vector3 TurnSpeed;
    private float trackingDecisionTime;
    private float trackingDecisionInterval = 0.5f;

    void Update()
    {
        if (Tools.IsPaused || Time.timeScale == 0f)
        {
            return;
        }

        if (Params.IsTracking)
        {
            trackingDecisionTime -= Time.deltaTime;
            if (trackingDecisionTime < 0f)
            {
                trackingDecisionTime += trackingDecisionInterval;
                FindTarget();
            }

            if (TrackTarget != null)
            {
                var direction = TrackTarget.transform.position - transform.position;
                var rightCheck = Vector3.Dot(transform.right, direction);
                if (rightCheck > 0)
                {
                    transform.Rotate(TurnSpeed * Time.deltaTime);
                }
                else
                {
                    transform.Rotate(-TurnSpeed * Time.deltaTime);
                }
            }
        }
        if (Spin)
        {
            ProjectileGO.transform.Rotate(SpinSpeed * Time.deltaTime);
        }
        var distance = Time.fixedDeltaTime * Velocity;
        transform.position = transform.position + (transform.forward * distance);
        RemainingLifespan -= distance;
        if (RemainingLifespan <= 0f)
        {
            Fizzle();
        }
    }

    public void FindTarget()
    {
        if (Params.TargetContainer == null)
        {
            return;
        }
        BlightCreature[] targets = Params.TargetContainer.GetComponentsInChildren<BlightCreature>();
        if (targets.Length == 0)
        {
            return;
        }

        BlightCreature closest = null;
        var closestDist = float.MaxValue;
        foreach (var target in targets)
        {
            var offset = target.transform.position - transform.position;
            var distance = offset.sqrMagnitude;
            if (distance < closestDist)
            {
                closest = target;
                closestDist = distance;
            }
        }

        if (closest != null)
        {
            TrackTarget = closest;
            TrackTarget.OnKilled += OnTrackedTargetKilledRecieved;
        }
    }

    public void OnTrackedTargetKilledRecieved(BlightCreature target)
    {
        if (TrackTarget == target)
        {
            TrackTarget = null;
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
        if (Spin)
        {
            if (SpinSpeed == Vector3.zero)
            {
                Spin = false;
            }
            else
            {
                SpinSpeed = new Vector3(
                    Random.Range(-SpinSpeed.x, SpinSpeed.x),  // Random spin on x-axis
                    Random.Range(-SpinSpeed.y, SpinSpeed.y),  // Random spin on y-axis
                    Random.Range(-SpinSpeed.z, SpinSpeed.z)   // Random spin on z-axis
                );
            }
        }
        if (projectileParams.IsTracking)
        {
            TurnSpeed = new Vector3(0, projectileParams.TurnSpeed, 0);
            trackingDecisionTime = trackingDecisionInterval;
        }
    }

    public void SetMaterial(Material material)
    {
        // Get the MeshRenderer component from the ProjectileGO
        if (ProjectileMeshRenderer == null)
        {
            ProjectileMeshRenderer = ProjectileGO.GetComponent<MeshRenderer>();
        }

        // Check if the MeshRenderer exists and has at least one material
        if (ProjectileMeshRenderer != null && ProjectileMeshRenderer.materials.Length > 0)
        {
            // Set the first material in the materials array
            Material[] materials = ProjectileMeshRenderer.sharedMaterials;

            // Modify the first material in the copied array
            materials[0] = material;

            // Reassign the modified array back to the sharedMaterials property
            ProjectileMeshRenderer.sharedMaterials = materials;
        }
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
            true,
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
        if (baseObject.TryGetComponent<Projectile>(out var projectile))
        {
            return;
        }
        if (baseObject.TryGetComponent<BlightCreature>(out var blightCreature))
        {
            if (blightCreature.IsDying)
            {
                return;
            }
        }
        if (baseObject.TryGetComponent<IMDamage>(out var damagable))
        {
            InflictDamage(damagable);
        }
        OnImpact(other);
    }
}
