using DG.Tweening;
using MalbersAnimations.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Weapon : MonoBehaviour
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

    [Header("In Game Values")]
    public WeaponPoolSO WeaponPool;
    public bool InUse;

    public MAnimal Wielder;
    public GameObject Muzzle;
    public GameObject ProjectileContainer;

    public bool FireImmediately;
    public float RateOfFire = 1f;
    public float Velocity = 0.75f;
    public int FollowShotCount = 0;
    public float FollowupShotSpeed = 0.2f;
    public int ParallelShots = 1;
    public float ParallelShotSpacing = 0.1f;
    public float ParallelShotAngle = 5;
    public bool IsFiring;

    [Header("Definition Values")]
    public ProjectilePoolSO ProjectilePool;

    private int _weaponLevel;
    public int WeaponLevel
    {
        get => _weaponLevel;
        set
        {
            _weaponLevel = value;
            SetLevelValues();
        }
    }
    public int ProjectileLevel;

    private Tween FireTween = null;
    private ObjectPool<FollowupTweenTracker> FollowupPool;
    private List<FollowupTweenTracker> FollowupTrackers;

    // Adjust stats based on projectile level.
    public virtual void SetLevelValues()
    {
        /*
        RateOfFire = 0.2f;
        Velocity = 1f;
        FollowShotCount = 0;
        FollowupShotSpeed = 0.01f;
        ParallelShots = 1;
        ParallelShotSpacing = 0.1f;
        ParallelShotAngle = 5f;
        */
    }

    // Do anything necessary after values have been set.
    public virtual void Initialize()
    {
    }

    private void OnDestroy()
    {
        StopAttacking();
    }

    public virtual void Equip(BlightCreature creature)//MAnimal wielder, GameObject muzzle, GameObject projectileContainer)
    {
        Wielder = creature.Wielder;
        Muzzle = creature.Muzzle;
        ProjectileContainer = creature.ProjectileContainer;

        var go = Wielder.gameObject;
        gameObject.layer = go.layer;
        transform.position = go.transform.position;

        Quaternion rotation = Quaternion.LookRotation(go.transform.forward, Vector3.up);
        
        Debug.Log("Rotation: " + transform.rotation);
        //transform.rotation = rotation * transform.rotation;
        Debug.Log("Rotation: " + transform.rotation);
        transform.SetParent(go.transform);
        creature.AddWeapon(this);
    }

    public virtual void ReturnToPool()
    {
        StopAttacking();
        WeaponPool.ReturnWeapon(this);
    }

    public void StartAttacking()
    {
        if (FireImmediately)
        {
            FireShots();
        }
        FireTween?.Kill();
        FireTween = DOVirtual.DelayedCall(RateOfFire, FireShots)
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
        IsFiring = true;
    }

    public void StopAttacking()
    {
        IsFiring = false;
        FireTween?.Kill();
        if (FollowupTrackers != null)
        {
            foreach (var followup in FollowupTrackers)
            {
                followup.StopAttacking();
            }
            FollowupTrackers.Clear();
        }
    }

    void FireShots()
    {
        CreateProjectiles();
        if (FollowShotCount > 0)
        {
            var followupTracker = FollowupPool.Get();
            followupTracker.StartAttacking(FollowupShotSpeed, FollowShotCount, FireShots);
            FollowupTrackers.Add(followupTracker);
        }
    }

    // Projectiles are fire and forget.  They are children of the parent of this object, and they know the pool
    // to return to.
    public void CreateProjectiles()
    {
        var muzzleTransform = Muzzle.transform;
        var rightStep = muzzleTransform.right * ParallelShotSpacing;
        var startPosition = muzzleTransform.position - (rightStep * (0.5f * (ParallelShots - 1)));
        var levelForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        var totalVelocity = Velocity;
        if (Wielder != null)
        {
            var totalMoveVector = Wielder.HorizontalVelocity + (levelForward * totalVelocity);
            totalVelocity = totalMoveVector.magnitude;
            levelForward = totalMoveVector.normalized;
        }
        // TODO: adjust the angle of levelForward for parallel shots.
        for (int i = 0; i < ParallelShots; i++)
        {
            var projectile = ProjectilePool.CreateProjectile(
                Wielder.gameObject,
                ProjectileContainer.transform,
                startPosition,
                levelForward,
                totalVelocity,
                ProjectileLevel);
            startPosition += rightStep;
        }
    }
}
