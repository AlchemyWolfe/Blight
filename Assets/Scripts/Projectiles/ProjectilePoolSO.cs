using MalbersAnimations;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/ProjectilePool", fileName = "SO_ProjectilePool_")]
public class ProjectilePoolSO : ScriptableObject
{
    public Projectile ProjectilePrefab;
    public StatID Stat;
    [HideInInspector]
    public ObjectPool<Projectile> ProjectilePool;
    private Material ProjectileMaterial;

    public void Initialize()
    {
        if (ProjectilePool == null)
        {
            ProjectilePool = new ObjectPool<Projectile>(OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile, false, 10, 100);
        }
        ProjectilePool.Clear();
    }

    public void SetProjectileMaterial(Material material)
    {
        ProjectileMaterial = material;
    }

    public Projectile CreateProjectile(GameObject attacker, Transform parent, Vector3 position, Vector3 forward, ProjectileParams projectileParams)
    {
        var projectile = ProjectilePool.Get();
        projectile.Attacker = attacker;
        projectile.transform.position = position;
        projectile.transform.SetParent(parent);
        projectile.Initialize(forward, projectileParams);
        // Finally, be on the layer of my parent.
        projectile.gameObject.layer = parent.gameObject.layer;
        if (ProjectileMaterial != null)
        {
            projectile.SetMaterial(ProjectileMaterial);
        }
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
