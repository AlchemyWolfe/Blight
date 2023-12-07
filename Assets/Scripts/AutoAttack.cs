using MalbersAnimations.Controller;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AutoAttack : MonoBehaviour
{
    private class FollowupShotData
    {
        public int remaining;
        public float nextShot;
    }

    [Tooltip("Type of projectile fired")]
    public ProjectilePoolSO ProjectileDefinition;

    [Tooltip("Parent for all projectiles.")]
    public GameObject ProjectileContainer;

    public MAnimal Weilder;

    [Tooltip("Seconds between bursts.  Should be multiples of 0.02f.")]
    [HorizontalGroup("ROF")]
    public float InitialRateOfFire = 0.5f;
    [Tooltip("How much to decrease RateOfFire per level.  Should be multiples of 0.02f.")]
    [HorizontalGroup("ROF")]
    public float RateOfFirePerLevel = 0.02f;
    [HideInInspector]
    public int RateOfFireLevel = 0;
    [HideInInspector]
    public float ActualRateOfFire;

    [Tooltip("Seconds between projectiles created per burst.  Should be multiples of 0.02f.")]
    public float FollowupShotSpeed = 0.04f;

    [Tooltip("Sound to play when the attack fires.")]
    public AudioClip FireSound;

    [Tooltip("Sound to play during followup projectiles are created in a burst.")]
    public AudioClip ExtraBurstSound;

    [Tooltip("The point to fire from, plus the projectile's initial orientation.")]
    public GameObject Muzzle;

    [Tooltip("The number of projectiles to fire after the original.")]
    [HorizontalGroup("Followup")]
    public int InitialFollowShotCount = 0;
    [Tooltip("How much to increase FollowShotCount per level.")]
    [HorizontalGroup("Followup")]
    public int FollowShotCountPerLevel = 1;
    [HideInInspector]
    public int FollowShotLevel = 0;
    [HideInInspector]
    public int ActualFollowShotCount;

    [Tooltip("The number of projectiles to fire at once, spaced evenly.")]
    [HorizontalGroup("Parallel")]
    public int InitialParallelShots = 1;
    [Tooltip("How much to increase ParallelShots per level.")]
    [HorizontalGroup("Parallel")]
    public int ParallelShotsPerLevel = 1;
    [HideInInspector]
    public int ParallelLevel = 0;
    [HideInInspector]
    public float ActualParallelShots;

    [Tooltip("Distance between parallel shots.")]
    [HorizontalGroup("Spacing")]
    public float ParallelShotSpacing = 0.1f;
    [Tooltip("How much to increase ParallelShotSpacing per Size level.")]
    [HorizontalGroup("Spacing")]
    public float ParallelShotSpacingPerLevel = 0f;
    [HideInInspector]
    public float ActualParallelShotSpacing;

    [Tooltip("Multiplier on damage and damage increases.")]
    public float DamageMultiplier = 1f;
    [HideInInspector]
    public int DamageLevel = 0;
    [HideInInspector]
    public float ActualDamage;

    [Tooltip("Multiplier on velocity and velocity increases.")]
    public float VelocityMultiplier = 1f;
    [HideInInspector]
    public int VelocityLevel = 0;
    [HideInInspector]
    public float ActualVelocity;

    [Tooltip("Multiplier on size and size increases.")]
    public float SizeMultiplier = 1f;
    [HideInInspector]
    public int SizeLevel = 0;
    [HideInInspector]
    public float ActualSize;

    public int Level { get { return RateOfFireLevel + FollowShotLevel + ParallelLevel + DamageLevel + VelocityLevel + SizeLevel + 1; } }
    private int MaxROFLevel;

    private float nextShot = 0f;
    private ObjectPool<FollowupShotData> FollowupPool;
    private List<FollowupShotData> FollowupShots;

    public class MyClass
    {
        public int Value;
        // Add other fields as needed

        // Reset method to clear or reset the instance
        public void Reset()
        {
            Value = 0;
            // Reset other fields as needed
        }
    }

    void Start()
    {
        MaxROFLevel = (int)(InitialRateOfFire / RateOfFirePerLevel);
        if ((RateOfFirePerLevel * MaxROFLevel) - InitialRateOfFire < 0.02f)
        {
            // Not allowing ROF of 0f.
            MaxROFLevel--;
        }
        AdjustRateOfFire(0);
        AdjustFollowShotCount(0);
        AdjustParallelShots(0);
        AdjustDamage(0);
        AdjustVelocity(0);
        AdjustSize(0);
        nextShot = Time.time + ActualRateOfFire;
        FollowupPool = new ObjectPool<FollowupShotData>(() => new FollowupShotData(), null, null);
        FollowupShots = new List<FollowupShotData>();
    }

    void FixedUpdate()
    {
        if (Time.time >= nextShot)
        {
            nextShot += ActualRateOfFire;
            CreateProjectiles();
            if (ActualFollowShotCount > 0)
            {
                var followupShotData = FollowupPool.Get();
                followupShotData.remaining = ActualFollowShotCount;
                followupShotData.nextShot = Time.time + FollowupShotSpeed;
                FollowupShots.Add(followupShotData);
            }
            return;
        }
        for (int i = FollowupShots.Count - 1; i >= 0; i--)
        {
            var followupShotData = FollowupShots[i];
            if (Time.time >= followupShotData.nextShot)
            {
                CreateProjectiles();
                if (followupShotData.remaining <= 1)
                {
                    // last shot fired
                    FollowupShots.RemoveAt(i);
                    FollowupPool.Release(followupShotData);
                }
                else
                {
                    followupShotData.nextShot += FollowupShotSpeed;
                    followupShotData.remaining--;
                }
            }
        }
    }

    // Projectiles are fire and forget.  They are children of the parent of this object, and they know the pool
    // to return to.
    public void CreateProjectiles()
    {
        var muzzleTransform = Muzzle.transform;
        var rightStep = muzzleTransform.right * ActualParallelShotSpacing;
        var startPosition = muzzleTransform.position - (rightStep * (0.5f * (ActualParallelShots - 1)));
        var levelForward = new Vector3(muzzleTransform.forward.x, 0f, muzzleTransform.forward.z).normalized;
        var levelVelocity = ActualVelocity;
        if (Weilder != null)
        {
            var totalMoveVector = Weilder.HorizontalVelocity + (levelForward * ActualVelocity);
            levelVelocity = totalMoveVector.magnitude;
            levelForward = totalMoveVector.normalized;
        }
        for (int i = 0; i < ActualParallelShots; i++)
        {
            var projectile = ProjectileDefinition.CreateProjectile(
                ProjectileContainer.transform,
                startPosition,
                levelForward,
                ActualDamage,
                levelVelocity,
                ActualSize);
        }
    }

    public void AdjustRateOfFire(int addLevels)
    {
        RateOfFireLevel = Mathf.Min(MaxROFLevel, RateOfFireLevel + addLevels);
        ActualRateOfFire = InitialRateOfFire - (RateOfFireLevel * RateOfFirePerLevel);
    }

    public void AdjustFollowShotCount(int addLevels)
    {
        FollowShotLevel += addLevels;
        ActualFollowShotCount = InitialFollowShotCount + (FollowShotLevel * FollowShotCountPerLevel);
    }

    public void AdjustParallelShots(int addLevels)
    {
        ParallelLevel += addLevels;
        ActualParallelShots = InitialParallelShots + (ParallelLevel * ParallelShotsPerLevel);
    }

    public void AdjustDamage(int addLevels)
    {
        DamageLevel += addLevels;
        ActualDamage = ProjectileDefinition.InitialDamage + (DamageLevel * ProjectileDefinition.DamagePerLevel);
    }

    public void AdjustVelocity(int addLevels)
    {
        VelocityLevel += addLevels;
        ActualVelocity = ProjectileDefinition.InitialVelocity + (VelocityLevel * ProjectileDefinition.VelocityPerLevel);
    }

    public void AdjustSize(int addLevels)
    {
        SizeLevel += addLevels;
        ActualSize = ProjectileDefinition.InitialSize + (SizeLevel * ProjectileDefinition.SizePerLevel);
        ActualParallelShotSpacing = ParallelShotSpacing + (SizeLevel * ParallelShotSpacingPerLevel);
    }
}
