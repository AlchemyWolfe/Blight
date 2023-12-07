using MalbersAnimations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/ProjectileDefinition", fileName = "SO_ProjectileDefinition_")]
public class ProjectilePoolSO : ScriptableObject
{
    [SerializeField]
    public Projectile ProjectilePrefab;

    [SerializeField]
    public StatID Stat;

    [SerializeField, HorizontalGroup("Damage")]
    [Tooltip("Damage the projectile does without any power increase.")]
    public float InitialDamage = 1f;

    [SerializeField, HorizontalGroup("Damage")]
    [Tooltip("Ammount to add when damage is increased.")]
    public float DamagePerLevel = 0.5f;

    [SerializeField, HorizontalGroup("Velocity")]
    [Tooltip("Speed the projectile moves without any power increase.")]
    public float InitialVelocity = 1f;

    [SerializeField, HorizontalGroup("Velocity")]
    [Tooltip("Ammount to add when velocity is increased.")]
    public float VelocityPerLevel = 0.1f;

    [SerializeField, HorizontalGroup("Size")]
    [Tooltip("Size multipler of the projectile without any power increase.")]
    public float InitialSize = 1f;

    [SerializeField, HorizontalGroup("Size")]
    [Tooltip("Ammount to add when size is increased.")]
    public float SizePerLevel = 0.2f;

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

    public Projectile CreateProjectile(Transform parent, Vector3 position, Vector3 forward, float damage, float velocity, float size)
    {
        var projectile = ProjectilePool.Get();
        projectile.transform.localScale = ProjectilePrefab.transform.localScale * size;
        projectile.transform.position = position;
        projectile.transform.forward = forward;
        projectile.transform.parent = parent;
        projectile.Damage = damage;
        projectile.Velocity = velocity;
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
        projectile.transform.parent = null;
        projectile.InUse = false;
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }
}
