using DG.Tweening;
using MalbersAnimations.Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public struct WeaponParams
{
    public float RateOfFire;
    public float Velocity;
    public int FollowShotCount;
    public float FollowupShotSpeed;
    public int ParallelShots;
    public float ParallelShotSpacing;
    public float ParallelShotAngle;
}

[Serializable]
public struct WeaponUpgradeParams
{
    public float FollowShotCount;
    public float ParallelShots;
    public bool MaintainSpreadAngle;
}

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

    [SerializeField]
    public string DisplayName;

    [SerializeField]
    private int _weaponLevel;
    public int WeaponLevel
    {
        get => _weaponLevel;
        private set
        {
            _weaponLevel = value;
            CalculateLeveledSpecs();
        }
    }

    [SerializeField]
    private int _projectileLevel;
    public int ProjectileLevel
    {
        get => _projectileLevel;
        private set
        {
            _projectileLevel = value;
            CalculateLeveledSpecs();
        }
    }

    [Header("In Game Values")]
    public WeaponPoolSO WeaponPool;
    public bool InUse;

    public MAnimal Wielder;
    public GameObject Muzzle;
    public GameObject ProjectileContainer;

    public bool FireImmediately;
    public ProjectileParams ProjectileSpecs;
    public ProjectleUpgradeParams ProjectileUpgrade;
    public WeaponParams WeaponSpecs;
    public WeaponUpgradeParams WeaponUpgrade;
    public bool IsFiring;

    [Header("Definition Values")]
    public ProjectilePoolSO ProjectilePool;

    [Header("Current Upgrades")]
    public ProjectileParams LeveledProjectileSpecs;
    public WeaponParams LeveledWeaponSpecs;

    private Tween FireTween = null;
    private ObjectPool<FollowupTweenTracker> FollowupPool;
    private List<FollowupTweenTracker> FollowupTrackers;

    // Adjust stats based on projectile level.
    public void SetLevelValues(int weaponLevel, int projectilelevel)
    {
        _weaponLevel = weaponLevel;
        _projectileLevel = projectilelevel;
        CalculateLeveledSpecs();
    }

    public void UpgradeWeapon()
    {
        WeaponLevel += 1;
        if (WeaponLevel == 1 && IsFiring)
        {
            StartAttacking();
        }

    }

    public void UpgradeProjectile()
    {
        ProjectileLevel += 1;
    }

    private void CalculateLeveledSpecs()
    {
        var addWeaponLevels = WeaponLevel - 1;
        LeveledWeaponSpecs.RateOfFire = WeaponSpecs.RateOfFire;
        LeveledWeaponSpecs.Velocity = WeaponSpecs.Velocity;
        LeveledWeaponSpecs.FollowShotCount = WeaponSpecs.FollowShotCount + (int)(WeaponUpgrade.FollowShotCount * addWeaponLevels);
        LeveledWeaponSpecs.FollowupShotSpeed = WeaponSpecs.FollowupShotSpeed;
        LeveledWeaponSpecs.ParallelShots = WeaponSpecs.ParallelShots + (int)(WeaponUpgrade.ParallelShots * addWeaponLevels);
        LeveledWeaponSpecs.ParallelShotSpacing = WeaponSpecs.ParallelShotSpacing;
        if (WeaponSpecs.ParallelShotAngle == 0f ||
            LeveledWeaponSpecs.ParallelShots <= 1 ||
            LeveledWeaponSpecs.ParallelShots == WeaponSpecs.ParallelShots)
        {
            // We are not adjusting angle, we're firing only one shot, or we haven't increased the number of parallel shots.
            // So use the default angle.
            LeveledWeaponSpecs.ParallelShotAngle = WeaponSpecs.ParallelShotAngle;
        }
        else if (WeaponUpgrade.MaintainSpreadAngle)
        {
            var originalSpread = (WeaponSpecs.ParallelShots - 1) * WeaponSpecs.ParallelShotAngle;
            LeveledWeaponSpecs.ParallelShotAngle = originalSpread / (LeveledWeaponSpecs.ParallelShots - 1);
        }
        else
        {
            // Leaving this else in case we do want to adjust angle on level increase sometime.
            LeveledWeaponSpecs.ParallelShotAngle = WeaponSpecs.ParallelShotAngle;
        }

        var addProjectileLevels = ProjectileLevel - 1;
        LeveledProjectileSpecs.Damage = ProjectileSpecs.Damage + (ProjectileUpgrade.Damage * addProjectileLevels);
        LeveledProjectileSpecs.Size = ProjectileSpecs.Size + (ProjectileUpgrade.Size * addProjectileLevels);
        LeveledProjectileSpecs.SizeY = ProjectileSpecs.SizeY;
        LeveledProjectileSpecs.Lifespan = ProjectileSpecs.Lifespan;
    }

    // Do anything necessary after values have been set.
    public virtual void Initialize()
    {
    }

    private void OnDestroy()
    {
        StopAttacking();
    }

    public virtual void Equip(BlightCreature creature)
    {
        Wielder = creature.Wielder;
        Muzzle = creature.Muzzle;
        ProjectileContainer = creature.ProjectileContainer;

        var go = Wielder.gameObject;
        gameObject.layer = go.layer;
        transform.position = go.transform.position;
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
        IsFiring = true;
        if (WeaponLevel <= 0)
        {
            return;
        }
        CalculateLeveledSpecs();
        if (FollowupPool == null)
        {
            FollowupPool = new ObjectPool<FollowupTweenTracker>(() => new FollowupTweenTracker(), null, null);
        }
        if (FollowupTrackers == null)
        {
            FollowupTrackers = new List<FollowupTweenTracker>();
        }
        if (FireImmediately)
        {
            FireShots();
        }
        FireTween?.Kill();
        FireTween = DOVirtual.DelayedCall(LeveledWeaponSpecs.RateOfFire, FireShots)
            .SetLoops(-1)
            .OnKill(() => FireTween = null);
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
        if (LeveledWeaponSpecs.FollowShotCount > 0)
        {
            var followupTracker = FollowupPool.Get();
            followupTracker.StartAttacking(LeveledWeaponSpecs.FollowupShotSpeed, LeveledWeaponSpecs.FollowShotCount, CreateProjectiles);
            FollowupTrackers.Add(followupTracker);
        }
    }

    // Projectiles are fire and forget.  They are children of the parent of this object, and they know the pool
    // to return to.
    public void CreateProjectiles()
    {
        var muzzleTransform = Muzzle.transform;
        var wielderForward = new Vector3(Wielder.HorizontalVelocity.x, 0f, Wielder.HorizontalVelocity.z);
        var levelForward = wielderForward.normalized;
        var angle = 0f;
        if (LeveledWeaponSpecs.ParallelShots > 1 && LeveledWeaponSpecs.ParallelShotAngle != 0f)
        {
            angle = -0.5f * (LeveledWeaponSpecs.ParallelShotAngle * (LeveledWeaponSpecs.ParallelShots - 1));
        }
        var startPosition = muzzleTransform.position;
        var rightStep = Vector3.zero;
        if (LeveledWeaponSpecs.ParallelShots > 1 && LeveledWeaponSpecs.ParallelShotSpacing != 0f)
        {
            var levelRight = new Vector3(levelForward.z, 0f, -levelForward.x);
            rightStep = levelRight * LeveledWeaponSpecs.ParallelShotSpacing;
            startPosition -= rightStep * (0.5f * (LeveledWeaponSpecs.ParallelShots - 1));
        }
        var projectileForward = levelForward * LeveledWeaponSpecs.Velocity;

        for (int i = 0; i < LeveledWeaponSpecs.ParallelShots; i++)
        {
            var rotatedForward = (angle == 0f) ? projectileForward : Quaternion.AngleAxis(angle, Vector3.up) * projectileForward;
            rotatedForward += wielderForward;
            var startOffset = rotatedForward.normalized * (LeveledProjectileSpecs.Size * 0.45f);

            var projectile = ProjectilePool.CreateProjectile(
                Wielder.gameObject,
                ProjectileContainer.transform,
                startPosition + startOffset,
                rotatedForward,
                LeveledProjectileSpecs);
            startPosition += rightStep;
            angle += LeveledWeaponSpecs.ParallelShotAngle;
        }
    }
}
