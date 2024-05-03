using MalbersAnimations;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/ProjectilePool", fileName = "SO_ProjectilePool_")]
public class ProjectilePoolSO : ScriptableObject
{
    [SerializeField]
    public Projectile ProjectilePrefab;

    [SerializeField]
    public StatID Stat;

    [SerializeField, HideInInspector]
    public ObjectPool<Projectile> ProjectilePool;

    public void Initialize()
    {
        if (ProjectilePool == null)
        {
            ProjectilePool = new ObjectPool<Projectile>(OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile, false, 10, 100);
        }
        ProjectilePool.Clear();
    }

    public Projectile CreateProjectile(GameObject attacker, Transform parent, Vector3 position, Vector3 forward, int level)
    {
        var projectile = ProjectilePool.Get();
        projectile.Attacker = attacker;
        projectile.transform.position = position;
        projectile.transform.forward = forward.normalized;
        projectile.transform.SetParent(parent);
        projectile.Velocity = forward.magnitude;
        projectile.Level = level;
        projectile.Initialize();
        // Finally, be on the layer of my parent.
        projectile.gameObject.layer = parent.gameObject.layer;
        return projectile;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        if (projectile.InUse)
        {
            ProjectilePool.Release(projectile);
        }
    }

    private Projectile OnCreateProjectile()
    {
        var projectile = GameObject.Instantiate(ProjectilePrefab);
        projectile.Pool = this;
        return projectile;
    }

    private void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
        projectile.InUse = true;
    }

    private void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.InUse = false;
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }
}
