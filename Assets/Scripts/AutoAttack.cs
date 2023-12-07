using DG.Tweening;
using MalbersAnimations.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AutoAttack
{
    private class FollowupTweenTracker
    {
        public Tween FireTween = null;

        public void StartAttacking(float RateOfFire, int ShotCount, TweenCallback callback)
        {
            FireTween?.Kill();
            FireTween = DOVirtual.DelayedCall(RateOfFire, callback)
                .SetLoops(ShotCount)
                .OnKill(() => FireTween = null);
        }

        public void StopAttacking()
        {
            if (FireTween != null)
            {
                FireTween.Kill();
                FireTween = null;
            }
        }
    }

    public AutoAttackSO AttackDefinition;
    public GameObject ProjectileContainer;
    public MAnimal Weilder;
    public GameObject Muzzle;

    public FloatPerLevel RateOfFire;
    public IntPerLevel FollowShotCount;
    public IntPerLevel ParallelShots;
    public FloatPerLevel Damage;
    public FloatPerLevel Velocity;
    public FloatPerLevel Size;
    public FloatPerLevel ParallelShotSpacing;
    public float ProjectileLifespan;
    private Tween FireTween = null;

    public int Level { get { return RateOfFire.Level + FollowShotCount.Level + ParallelShots.Level + Damage.Level + Velocity.Level + Size.Level + 1; } }

    private ObjectPool<FollowupTweenTracker> FollowupPool;
    private List<FollowupTweenTracker> FollowupTrackers;

    public AutoAttack(AutoAttackSO attackDefinition,
        MAnimal weilder,
        GameObject muzzle,
        GameObject projectileContainer,
        int rateOfFireLevel,
        int followShotLevel,
        int parallelLevel,
        int damageLevel,
        int velocityLevel,
        int sizeLevel)
    {
        AttackDefinition = attackDefinition;
        Weilder = weilder;
        Muzzle = muzzle;
        ProjectileContainer = projectileContainer;
        RateOfFire = AttackDefinition.RateOfFire;
        RateOfFire.SetLevel(rateOfFireLevel);
        FollowShotCount = attackDefinition.FollowShotCount;
        FollowShotCount.SetLevel(followShotLevel);
        ParallelShots = attackDefinition.ParallelShots;
        ParallelShots.SetLevel(parallelLevel);
        Damage = AttackDefinition.ProjectileDefinition.Damage;
        Damage.ScaleValues(AttackDefinition.DamageMultiplier);
        Damage.SetLevel(damageLevel);
        Velocity = AttackDefinition.ProjectileDefinition.Velocity;
        Velocity.ScaleValues(AttackDefinition.VelocityMultiplier);
        Velocity.SetLevel(velocityLevel);
        Size = AttackDefinition.ProjectileDefinition.Size;
        Size.ScaleValues(AttackDefinition.SizeMultiplier);
        Size.SetLevel(sizeLevel);
        ParallelShotSpacing = AttackDefinition.ParallelShotSpacing;
        ParallelShotSpacing.SetLevel(sizeLevel);
        ProjectileLifespan = attackDefinition.LifespanMultiplier * AttackDefinition.ProjectileDefinition.Lifespan;
    }

    public void StartAttacking()
    {
        if (AttackDefinition.FireImmediately)
        {
            FireShots();
        }
        FireTween?.Kill();
        FireTween = DOVirtual.DelayedCall(RateOfFire.Value, FireShots)
            .SetLoops(-1)
            .OnKill(() => FireTween = null);
        if (FollowupPool != null)
        {
            FollowupPool = new ObjectPool<FollowupTweenTracker>(() => new FollowupTweenTracker(), null, null);
        }
        if (FollowupTrackers != null)
        {
            FollowupTrackers = new List<FollowupTweenTracker>();
        }
    }

    public void StopAttacking()
    {
        FireTween.Kill();
        foreach (var followup in FollowupTrackers)
        {
            followup.StopAttacking();
        }
        FollowupTrackers.Clear();
    }

    void FireShots()
    {
        CreateProjectiles();
        if (FollowShotCount.Value > 0)
        {
            var followupTracker = FollowupPool.Get();
            followupTracker.StartAttacking(AttackDefinition.FollowupShotSpeed, FollowShotCount.Value, FireShots);
            FollowupTrackers.Add(followupTracker);
        }
    }

    // Projectiles are fire and forget.  They are children of the parent of this object, and they know the pool
    // to return to.
    public void CreateProjectiles()
    {
        var muzzleTransform = Muzzle.transform;
        var rightStep = muzzleTransform.right * ParallelShotSpacing.Value;
        var startPosition = muzzleTransform.position - (rightStep * (0.5f * (ParallelShots.Value - 1)));
        var levelForward = new Vector3(muzzleTransform.forward.x, 0f, muzzleTransform.forward.z).normalized;
        var totalVelocity = Velocity.Value;
        if (Weilder != null)
        {
            var totalMoveVector = Weilder.HorizontalVelocity + (levelForward * totalVelocity);
            totalVelocity = totalMoveVector.magnitude;
            levelForward = totalMoveVector.normalized;
        }
        for (int i = 0; i < ParallelShots.Value; i++)
        {
            var projectile = AttackDefinition.ProjectileDefinition.CreateProjectile(
                Weilder.gameObject,
                ProjectileContainer.transform,
                startPosition,
                levelForward,
                Damage.Value,
                totalVelocity,
                Size.Value,
                ProjectileLifespan);
        }
    }
}
